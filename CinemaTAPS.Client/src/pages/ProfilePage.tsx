import { useState, useEffect, useCallback } from 'react';
import toast from 'react-hot-toast';
import { apiClient } from '../api/apiClient';

interface Reservation {
  id: number;
  rowPosition: number;
  seatPosition: number;
  screening?: {
    movieTitle: string;
    startTime: string;
  };
}

interface UserProfile {
  id: number;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  role: string;
  concurrencyToken: string;
  reservations: Reservation[];
}

export const ProfilePage = () => {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({ firstName: '', lastName: '', phoneNumber: '' });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    apiClient.get<UserProfile>('/users/me')
      .then((data) => {
        setProfile(data);
        setFormData({
          firstName: data.firstName,
          lastName: data.lastName,
          phoneNumber: data.phoneNumber,
        });
      })
      .catch((err: any) => toast.error(err.message || 'Failed to load profile.'))
      .finally(() => setLoading(false));
  }, []);

  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  }, []);

  const handleSave = useCallback(async () => {
    if (!profile || saving) return;
    setSaving(true);
    try {
      const updated = await apiClient.put<UserProfile>('/users/me', {
        ...formData,
        concurrencyToken: profile.concurrencyToken,
      });
      setProfile(updated);
      setFormData({
        firstName: updated.firstName,
        lastName: updated.lastName,
        phoneNumber: updated.phoneNumber,
      });
      setIsEditing(false);
      toast.success('Profile updated successfully.');
    } catch (err: any) {
      if (err.status === 409) {
        toast.error('Concurrency conflict: profile was modified by someone else. Refresh and try again.');
      } else {
        toast.error(err.message || 'Failed to update profile.');
      }
    } finally {
      setSaving(false);
    }
  }, [profile, formData, saving]);

  const handleCancelReservation = useCallback(async (reservationId: number) => {
    if (!window.confirm('Cancel this reservation?')) return;
    try {
      await apiClient.delete(`/reservations/${reservationId}`);
      setProfile((prev) =>
        prev ? { ...prev, reservations: prev.reservations.filter((r) => r.id !== reservationId) } : prev
      );
      toast.success('Reservation cancelled.');
    } catch (err: any) {
      toast.error(err.message || 'Failed to cancel reservation.');
    }
  }, []);

  if (loading) {
    return (
      <div className="container mt-5">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="container mt-5">
        <div className="alert alert-danger">Profile not found.</div>
      </div>
    );
  }

  return (
    <div className="container mt-5">
      <div className="row">
        <div className="col-md-8">
          <h2 className="mb-4">My Profile</h2>

          <div className="card mb-4">
            <div className="card-body">
              {!isEditing ? (
                <>
                  <div className="mb-3">
                    <label className="form-label text-muted">Email</label>
                    <p>{profile.email}</p>
                  </div>
                  <div className="mb-3">
                    <label className="form-label text-muted">First Name</label>
                    <p>{profile.firstName}</p>
                  </div>
                  <div className="mb-3">
                    <label className="form-label text-muted">Last Name</label>
                    <p>{profile.lastName}</p>
                  </div>
                  <div className="mb-3">
                    <label className="form-label text-muted">Phone</label>
                    <p>{profile.phoneNumber}</p>
                  </div>
                  <button className="btn btn-primary" onClick={() => setIsEditing(true)}>
                    Edit Profile
                  </button>
                </>
              ) : (
                <>
                  <div className="mb-3">
                    <label className="form-label">First Name</label>
                    <input type="text" className="form-control" name="firstName"
                      value={formData.firstName} onChange={handleChange} />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Last Name</label>
                    <input type="text" className="form-control" name="lastName"
                      value={formData.lastName} onChange={handleChange} />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Phone</label>
                    <input type="tel" className="form-control" name="phoneNumber"
                      value={formData.phoneNumber} onChange={handleChange} />
                  </div>
                  <button className="btn btn-success" onClick={handleSave} disabled={saving}>
                    {saving ? 'Saving...' : 'Save Changes'}
                  </button>
                  <button className="btn btn-secondary ms-2" onClick={() => setIsEditing(false)} disabled={saving}>
                    Cancel
                  </button>
                </>
              )}
            </div>
          </div>

          <h3 className="mb-3">My Reservations</h3>
          {profile.reservations.length > 0 ? (
            <div className="table-responsive">
              <table className="table table-striped">
                <thead>
                  <tr>
                    <th>Movie</th>
                    <th>Seat</th>
                    <th>Time</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {profile.reservations.map((res) => (
                    <tr key={res.id}>
                      <td>{res.screening?.movieTitle ?? '—'}</td>
                      <td>Row {res.rowPosition}, Seat {res.seatPosition}</td>
                      <td>{res.screening ? new Date(res.screening.startTime).toLocaleString() : '—'}</td>
                      <td>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => handleCancelReservation(res.id)}
                        >
                          Cancel
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="alert alert-info">No reservations yet.</div>
          )}
        </div>
      </div>
    </div>
  );
};
