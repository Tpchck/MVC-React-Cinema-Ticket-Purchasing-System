import React, { useState } from 'react';
import '../styles/SeatMap.css';

interface Seat {
  row: number;
  position: number;
  status: 'Free' | 'Occupied' | 'YourSeat';
  reservationId?: number;
  reservedByUserId?: number;
}

interface SeatMapProps {
  rows: number;
  seatsPerRow: number;
  seats: Seat[];
  onSeatSelect?: (row: number, position: number) => void;
  onSeatCancel?: (reservationId: number) => void;
  currentUserId?: number;
  editable?: boolean;
}

export const SeatMap: React.FC<SeatMapProps> = ({
  seatsPerRow,
  seats,
  onSeatSelect,
  onSeatCancel,
  editable = true,
}) => {
  const [selectedSeats, setSelectedSeats] = useState<Array<{ row: number; pos: number }>>([]);

  const handleSeatClick = (seat: Seat) => {
    if (!editable) return;

    if (seat.status === 'Occupied') return;

    if (seat.status === 'YourSeat' && onSeatCancel && seat.reservationId) {
      onSeatCancel(seat.reservationId);
      return;
    }

    if (seat.status === 'Free') {
      const isSelected = selectedSeats.some(s => s.row === seat.row && s.pos === seat.position);
      if (isSelected) {
        setSelectedSeats(selectedSeats.filter(s => !(s.row === seat.row && s.pos === seat.position)));
      } else {
        setSelectedSeats([...selectedSeats, { row: seat.row, pos: seat.position }]);
      }
    }
  };

  const handleConfirmSelection = () => {
    if (selectedSeats.length > 0 && onSeatSelect) {
      selectedSeats.forEach(seat => {
        onSeatSelect(seat.row, seat.pos);
      });
      setSelectedSeats([]);
    }
  };

  const getSeatClass = (seat: Seat): string => {
    if (seat.status === 'Occupied') return 'seat occupied';
    if (seat.status === 'YourSeat') return 'seat your-seat';
    if (selectedSeats.some(s => s.row === seat.row && s.pos === seat.position)) {
      return 'seat selected';
    }
    return 'seat free';
  };

  return (
    <div className="seatmap-container">
      <div className="seatmap-legend mb-3">
        <div className="legend-item">
          <div className="seat free"></div>
          <span>Free</span>
        </div>
        <div className="legend-item">
          <div className="seat your-seat"></div>
          <span>Your Seat</span>
        </div>
        <div className="legend-item">
          <div className="seat occupied"></div>
          <span>Occupied</span>
        </div>
      </div>

      <div className="seatmap-grid" style={{
        gridTemplateColumns: `repeat(${seatsPerRow}, 1fr)`,
        gap: '8px',
        padding: '20px',
        border: '1px solid #ddd',
        borderRadius: '8px',
        backgroundColor: '#f9f9f9',
      }}>
        {seats.map((seat) => (
          <button
            key={`${seat.row}-${seat.position}`}
            className={getSeatClass(seat)}
            onClick={() => handleSeatClick(seat)}
            disabled={!editable || seat.status === 'Occupied'}
            title={`Row ${seat.row}, Seat ${seat.position}`}
          >
            {seat.row}-{seat.position}
          </button>
        ))}
      </div>

      {selectedSeats.length > 0 && editable && (
        <div className="mt-3">
          <p>
            Selected seats ({selectedSeats.length}): 
            <strong className="ms-2">
              {selectedSeats.map(s => `${s.row}-${s.pos}`).join(', ')}
            </strong>
          </p>
          <button
            className="btn btn-success"
            onClick={handleConfirmSelection}
          >
            Book {selectedSeats.length} {selectedSeats.length === 1 ? 'Seat' : 'Seats'}
          </button>
          <button
            className="btn btn-secondary ms-2"
            onClick={() => setSelectedSeats([])}
          >
            Clear Selection
          </button>
        </div>
      )}
    </div>
  );
};
