import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useAuthStore } from '../store/authStore';
import { SeatMap } from '../components/SeatMap';
import { apiClient } from '../api/apiClient';

interface Seat {
  row: number;
  position: number;
  status: 'Free' | 'Occupied' | 'YourSeat';
  reservationId?: number;
  reservedByUserId?: number;
}

interface SeatMapData {
  screeningId: number;
  screening: {
    id: number;
    movieTitle: string;
    startTime: string;
    cinemaId: number;
    cinema: {
      id: number;
      name: string;
      rows: number;
      seatsPerRow: number;
    };
  };
  rows: number;
  seatsPerRow: number;
  seats: Seat[];
}

export const SeatMapPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuthStore();
  const [seatMap, setSeatMap] = useState<SeatMapData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchSeatMap = async () => {
      try {
        const data = await apiClient.get<SeatMapData>(`/reservations/seatmap/${id}`);
        setSeatMap(data);
      } catch (err: any) {
        toast.error(err.message || 'Failed to load seat map.');
      } finally {
        setLoading(false);
      }
    };

    fetchSeatMap();
  }, [id]);

  const [isBooking, setIsBooking] = useState(false);

  const handleSeatSelect = async (row: number, position: number) => {
    if (!isAuthenticated) {
      navigate('/auth/login');
      return;
    }

    setError('');
    setSuccess('');
    setIsBooking(true);

    try {
      await apiClient.post('/reservations/book', {
        screeningId: parseInt(id!),
        rowPosition: row,
        seatPosition: position,
      });
      toast.success(`Seat ${row}-${position} booked successfully.`);
    } catch (err: any) {
      if (err.status === 409) {
        toast.error(
          `Concurrency conflict: seat ${row}-${position} was just taken by another user. ` +
          `The seat map has been refreshed — please choose a different seat.`,
          { duration: 6000 }
        );
      } else if (err.status === 401) {
        navigate('/auth/login');
        return;
      } else {
        toast.error(err.message || 'Failed to book seat. Please try again.');
      }
    } finally {
      setIsBooking(false);
      // Always refresh seat map to show current real state
      try {
        const data = await apiClient.get<SeatMapData>(`/reservations/seatmap/${id}`);
        setSeatMap(data);
      } catch {
        // ignore refresh error
      }
    }
  };

  const handleSeatCancel = async (reservationId: number) => {
    if (!window.confirm('Are you sure you want to cancel this reservation?')) {
      return;
    }

    try {
      await apiClient.delete(`/reservations/${reservationId}`);
      toast.success('Reservation cancelled.');
      const data = await apiClient.get<SeatMapData>(`/reservations/seatmap/${id}`);
      setSeatMap(data);
    } catch (err: any) {
      toast.error(err.message || 'Failed to cancel reservation.');
    }
  };

  if (loading) {
    return (
      <div className="container mt-5">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (!seatMap) {
    return (
      <div className="container mt-5">
        <div className="alert alert-danger">Seat map not found</div>
      </div>
    );
  }

  const startTime = new Date(seatMap.screening.startTime).toLocaleString();

  return (
    <div className="container mt-5">
      <button className="btn btn-secondary mb-3" onClick={() => navigate('/')}>
        ← Back to Screenings
      </button>
      
      <div className="card mb-4">
        <div className="card-body">
          <h2>{seatMap.screening.movieTitle}</h2>
          <p className="text-muted mb-1">Cinema: {seatMap.screening.cinema.name}</p>
          <p className="text-muted">Time: {startTime}</p>
        </div>
      </div>

      {isBooking && (
        <div className="alert alert-info d-flex align-items-center gap-2 mb-3">
          <div className="spinner-border spinner-border-sm" role="status" />
          <span>Booking seat, please wait…</span>
        </div>
      )}


      <SeatMap
        rows={seatMap.rows}
        seatsPerRow={seatMap.seatsPerRow}
        seats={seatMap.seats}
        onSeatSelect={handleSeatSelect}
        onSeatCancel={handleSeatCancel}
        currentUserId={user?.id}
        editable={isAuthenticated && !isBooking}
      />
    </div>
  );
};
