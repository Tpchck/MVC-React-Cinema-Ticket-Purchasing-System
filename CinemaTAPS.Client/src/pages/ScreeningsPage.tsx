import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../api/apiClient';

interface Screening {
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
}

const ScreeningCard = ({ screening }: { screening: Screening }) => {
  const navigate = useNavigate();
  const startTime = new Date(screening.startTime).toLocaleString();

  return (
    <div className="card h-100">
      <div className="card-body">
        <h5 className="card-title">{screening.movieTitle}</h5>
        <p className="card-text">
          <strong>Cinema:</strong> {screening.cinema.name}
        </p>
        <p className="card-text">
          <strong>Time:</strong> {startTime}
        </p>
        <p className="card-text text-muted">
          {screening.cinema.rows} rows × {screening.cinema.seatsPerRow} seats
        </p>
        <button
          className="btn btn-primary"
          onClick={() => navigate(`/screenings/${screening.id}/seats`)}
        >
          View Seats
        </button>
      </div>
    </div>
  );
};

export const ScreeningsPage = () => {
  const [screenings, setScreenings] = useState<Screening[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchScreenings = async () => {
      try {
        const data = await apiClient.get<Screening[]>('/screenings');
        setScreenings(data);
      } catch (err: any) {
        setError(err.message || 'Failed to load screenings');
      } finally {
        setLoading(false);
      }
    };

    fetchScreenings();
  }, []);

  if (loading) return <div className="container mt-5"><div className="spinner-border" role="status"><span className="visually-hidden">Loading...</span></div></div>;
  if (error) return <div className="container mt-5"><div className="alert alert-danger">{error}</div></div>;

  return (
    <div className="container mt-5">
      <h1 className="mb-4">Available Screenings</h1>
      <div className="row g-4">
        {screenings.map((screening) => (
          <div key={screening.id} className="col-md-6 col-lg-4">
            <ScreeningCard screening={screening} />
          </div>
        ))}
      </div>
      {screenings.length === 0 && (
        <div className="alert alert-info">No screenings available</div>
      )}
    </div>
  );
};
