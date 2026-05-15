import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import { apiClient } from '../api/apiClient';

interface Screening {
  id: number;
  movieTitle: string;
  startTime: string;
  cinema: { name: string };
}

interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  phoneNumber?: string;
  concurrencyToken?: string;
}

export const AdminDashboardPage = () => {
  const [screenings, setScreenings] = useState<Screening[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [editFormData, setEditFormData] = useState({ firstName: '', lastName: '', phoneNumber: '' });

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [screeningsData, usersData] = await Promise.all([
          apiClient.get<Screening[]>('/screenings'),
          apiClient.get<User[]>('/admin/users'),
        ]);
        setScreenings(screeningsData);
        setUsers(usersData);
      } catch (err: any) {
        toast.error(err.message || 'Failed to load data.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleDeleteScreening = async (id: number) => {
    if (!window.confirm('Delete this screening?')) return;
    try {
      await apiClient.delete(`/screenings/${id}`);
      setScreenings(screenings.filter(s => s.id !== id));
      toast.success('Screening deleted.');
    } catch (err: any) {
      toast.error(err.message || 'Failed to delete screening.');
    }
  };

  const handleEditUser = (user: User) => {
    setEditingUser(user);
    setEditFormData({
      firstName: user.firstName,
      lastName: user.lastName,
      phoneNumber: user.phoneNumber || ''
    });
  };

  const handleSaveUser = async () => {
    if (!editingUser) return;
    try {
      await apiClient.put(`/admin/users/${editingUser.id}`, {
        firstName: editFormData.firstName,
        lastName: editFormData.lastName,
        phoneNumber: editFormData.phoneNumber,
        concurrencyToken: editingUser.concurrencyToken
      });
      const updatedUsers = users.map(u => 
        u.id === editingUser.id 
          ? { ...u, ...editFormData }
          : u
      );
      setUsers(updatedUsers);
      setEditingUser(null);
      toast.success('User updated.');
    } catch (err: any) {
      toast.error(err.message || 'Failed to update user.');
    }
  };

  if (loading) return <div className="container mt-5"><div className="spinner-border"></div></div>;

  return (
    <div className="container mt-5">
      <h1>Admin Dashboard</h1>

      <h3 className="mt-5">Screenings</h3>
      <div className="table-responsive">
        <table className="table table-striped">
          <thead>
            <tr>
              <th>Movie</th>
              <th>Cinema</th>
              <th>Time</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {screenings.map(s => (
              <tr key={s.id}>
                <td>{s.movieTitle}</td>
                <td>{s.cinema.name}</td>
                <td>{new Date(s.startTime).toLocaleString()}</td>
                <td>
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => handleDeleteScreening(s.id)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <h3 className="mt-5">Users</h3>
      <div className="table-responsive">
        <table className="table table-striped">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Phone</th>
              <th>Role</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {users.map(u => (
              <tr key={u.id}>
                <td>{u.firstName} {u.lastName}</td>
                <td>{u.email}</td>
                <td>{u.phoneNumber || '-'}</td>
                <td>{u.role}</td>
                <td>
                  <button
                    className="btn btn-sm btn-primary"
                    onClick={() => handleEditUser(u)}
                  >
                    Edit
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {editingUser && (
        <div 
          className="modal d-block" 
          style={{ backgroundColor: 'rgba(0,0,0,0.5)', display: editingUser ? 'block' : 'none' }}
        >
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit User: {editingUser.firstName} {editingUser.lastName}</h5>
                <button type="button" className="btn-close" onClick={() => setEditingUser(null)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">First Name</label>
                  <input
                    type="text"
                    className="form-control"
                    value={editFormData.firstName}
                    onChange={(e) => setEditFormData({...editFormData, firstName: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Last Name</label>
                  <input
                    type="text"
                    className="form-control"
                    value={editFormData.lastName}
                    onChange={(e) => setEditFormData({...editFormData, lastName: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Phone</label>
                  <input
                    type="tel"
                    className="form-control"
                    value={editFormData.phoneNumber}
                    onChange={(e) => setEditFormData({...editFormData, phoneNumber: e.target.value})}
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={() => setEditingUser(null)}>Cancel</button>
                <button className="btn btn-primary" onClick={handleSaveUser}>Save</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
