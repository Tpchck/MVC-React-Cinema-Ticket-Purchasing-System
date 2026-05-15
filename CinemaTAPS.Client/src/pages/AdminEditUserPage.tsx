import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { apiClient } from '../api/apiClient';

interface Reservation {
  id: number;
  rowPosition: number;
  seatPosition: number;
  screeningId: number;
  screening?: {
    id: number;
    movieTitle: string;
    startTime: string;
    cinemaId: number;
  };
}

interface UserDetail {
  id: number;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  role: string;
  concurrencyToken: string;
  reservations: Reservation[];
}

export const AdminEditUserPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [user, setUser] = useState<UserDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    phoneNumber: '',
  });

  // Per-reservation new seat inputs: { [reservationId]: "row_seat" }
  const [seatInputs, setSeatInputs] = useState<Record<number, string>>({});

  useEffect(() => {
    if (!id) return;
    apiClient
      .get<UserDetail>(`/admin/users/${id}`)
      .then((data) => {
        setUser(data);
        setFormData({
          firstName: data.firstName,
          lastName: data.lastName,
          phoneNumber: data.phoneNumber,
        });
        // Init seat inputs with current values
        const initial: Record<number, string> = {};
        data.reservations.forEach((r) => {
          initial[r.id] = `${r.rowPosition}_${r.seatPosition}`;
        });
        setSeatInputs(initial);
      })
      .catch((err: any) => toast.error(err.message || 'Failed to load user.'))
      .finally(() => setLoading(false));
  }, [id]);

  const handleSaveProfile = async () => {
    if (!user) return;
    setSaving(true);
    try {
      const updated = await apiClient.put<UserDetail>(`/admin/users/${user.id}`, {
        firstName: formData.firstName,
        lastName: formData.lastName,
        phoneNumber: formData.phoneNumber,
        concurrencyToken: user.concurrencyToken,
      });
      setUser(updated);
      toast.success('Profile updated successfully.');
    } catch (err: any) {
      if (err.status === 409) {
        toast.error('Concurrency conflict: someone else edited this user. Refresh and try again.');
      } else {
        toast.error(err.message || 'Failed to update profile.');
      }
    } finally {
      setSaving(false);
    }
  };

  const handleChangeSeat = async (reservationId: number) => {
    const value = seatInputs[reservationId] || '';
    const parts = value.split('_');
    if (parts.length !== 2 || isNaN(+parts[0]) || isNaN(+parts[1])) {
      toast.error('Invalid seat format. Use row_seat (e.g. 3_5).');
      return;
    }
    try {
      await apiClient.put(`/admin/reservations/${reservationId}/seat`, {
        rowPosition: +parts[0],
        seatPosition: +parts[1],
      });
      toast.success(`Seat changed to Row ${parts[0]}, Seat ${parts[1]}.`);
      const updated = await apiClient.get<UserDetail>(`/admin/users/${id}`);
      setUser(updated);
    } catch (err: any) {
      toast.error(err.message || 'Failed to change seat.');
    }
  };

  const handleDeleteReservation = async (reservationId: number) => {
    if (!window.confirm('Delete this reservation?')) return;
    try {
      await apiClient.delete(`/admin/reservations/${reservationId}`);
      setUser((prev) =>
        prev
          ? { ...prev, reservations: prev.reservations.filter((r) => r.id !== reservationId) }
          : prev
      );
      toast.success('Reservation deleted.');
    } catch (err: any) {
      toast.error(err.message || 'Failed to delete reservation.');
    }
  };

  if (loading)
    return (
      <div className="container mt-5 text-center">
        <div className="spinner-border" role="status" />
      </div>
    );

  if (!user)
    return (
      <div className="container mt-5">
        <div className="alert alert-danger">User not found.</div>
        <button className="btn btn-secondary" onClick={() => navigate('/admin')}>
          ← Back to Dashboard
        </button>
      </div>
    );

  return (
    <div className="container mt-5">
      <button className="btn btn-outline-secondary mb-3" onClick={() => navigate('/admin')}>
        ← Back to Dashboard
      </button>

      <h2>
        Edit User: {user.firstName} {user.lastName}
      </h2>
      <p className="text-muted">{user.email} — Role: {user.role}</p>

      {/* Profile Form */}
      <div className="card mb-5">
        <div className="card-header">Profile</div>
        <div className="card-body">
          <div className="row g-3">
            <div className="col-md-6">
              <label className="form-label">First Name</label>
              <input
                type="text"
                className="form-control"
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">Last Name</label>
              <input
                type="text"
                className="form-control"
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">Phone Number</label>
              <input
                type="tel"
                className="form-control"
                value={formData.phoneNumber}
                onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
              />
            </div>
          </div>
          <button
            className="btn btn-primary mt-3"
            onClick={handleSaveProfile}
            disabled={saving}
          >
            {saving ? 'Saving…' : 'Save Profile'}
          </button>
        </div>
      </div>

      {/* Reservations */}
      <h4>Reservations ({user.reservations.length})</h4>
      {user.reservations.length === 0 ? (
        <p className="text-muted">No reservations.</p>
      ) : (
        <div className="table-responsive">
          <table className="table table-striped align-middle">
            <thead>
              <tr>
                <th>Movie</th>
                <th>Date</th>
                <th>Current Seat</th>
                <th>New Seat (row_seat)</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {user.reservations.map((r) => (
                <tr key={r.id}>
                  <td>{r.screening?.movieTitle ?? '—'}</td>
                  <td>
                    {r.screening
                      ? new Date(r.screening.startTime).toLocaleString()
                      : '—'}
                  </td>
                  <td>
                    Row {r.rowPosition}, Seat {r.seatPosition}
                  </td>
                  <td>
                    <input
                      type="text"
                      className="form-control form-control-sm"
                      style={{ width: 110 }}
                      placeholder="e.g. 3_5"
                      value={seatInputs[r.id] ?? ''}
                      onChange={(e) =>
                        setSeatInputs({ ...seatInputs, [r.id]: e.target.value })
                      }
                    />
                  </td>
                  <td className="d-flex gap-2">
                    <button
                      className="btn btn-sm btn-warning"
                      onClick={() => handleChangeSeat(r.id)}
                    >
                      Change Seat
                    </button>
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => handleDeleteReservation(r.id)}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};
