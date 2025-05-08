import React, { useState, useEffect, useCallback } from 'react';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { addNotification } from '../../../store/slices/uiSlice';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { FaEdit, FaEye } from 'react-icons/fa';
import styled from 'styled-components';
import Select from 'react-select';
import { Link } from 'react-router-dom';

const Container = styled.div`background-color: var(--bg-color-secondary); border-radius: 8px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);`;
const Header = styled.div`display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; h2 { margin: 0; }`;
const Table = styled.table`width: 100%; border-collapse: collapse; th, td { padding: 10px; border-bottom: 1px solid var(--bg-color-tertiary); } th { color: var(--text-color-secondary); } td button { margin-right: 8px; }`;
const ButtonGroup = styled.div`display: flex; gap: 8px;`;
const ModalOverlay = styled.div`position: fixed; inset: 0; background: rgba(0,0,0,0.6); display: flex; justify-content: center; align-items: center; z-index: 1000;`;
const ModalContent = styled.div`background: var(--bg-color-secondary); padding: 30px; border-radius: 8px; min-width: 400px;`;
const Field = styled.div`margin-bottom: 15px; label { display: block; margin-bottom: 5px; color: var(--text-color-secondary); }`;
const Input = styled.input`width: 100%; padding: 8px; border: 1px solid var(--bg-color-tertiary); border-radius: 4px; background: var(--bg-color); color: var(--text-color);`;
const Checkbox = styled.input`margin-right: 8px;`;
const Actions = styled.div`display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px;`;

const roleOptions = [
  { value: 'Admin', label: 'Admin' },
  { value: 'Moderator', label: 'Moderator' }
];

const UsersManagement = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [current, setCurrent] = useState(null);
  const [form, setForm] = useState({ userId: '', roles: [], isActive: false });
  const dispatch = useDispatch();

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    try {
      const res = await axios.get('/api/admin/manage/users');
      setUsers(res.data || []);
    } catch (err) {
      console.error(err);
      setError('Failed to load users.');
      dispatch(addNotification({ message: 'Failed to load users.', type: 'error' }));
    } finally {
      setLoading(false);
    }
  }, [dispatch]);

  useEffect(() => { fetchUsers(); }, [fetchUsers]);

  const openModal = (user) => {
    setCurrent(user);
    setForm({ userId: user.userId, roles: user.roles, isActive: user.isActive });
    setIsModalOpen(true);
    setError(null);
  };
  const closeModal = () => { setIsModalOpen(false); setCurrent(null); setError(null); };

  const handleSave = async () => {
    try {
      await axios.put(`/api/admin/manage/users/${form.userId}`, { roles: form.roles, isActive: form.isActive });
      dispatch(addNotification({ message: 'Користувача оновлено', type: 'success' }));
      closeModal(); fetchUsers();
    } catch (err) {
      console.error(err);
      const msg = err.response?.data?.message || 'Error updating user.';
      setError(msg);
      dispatch(addNotification({ message: msg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <Container>
      <Header><h2>Users Management</h2></Header>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <Table>
        <thead>
          <tr><th>ID</th><th>UserName</th><th>Email</th><th>Roles</th><th>Active</th><th>Actions</th></tr>
        </thead>
        <tbody>
          {users.map(u => (
            <tr key={u.userId}>
              <td>{u.userId}</td>
              <td>{u.userName}</td>
              <td>{u.email}</td>
              <td>{u.roles.join(', ')}</td>
              <td>{u.isActive ? 'Yes' : 'No'}</td>
              <td><ButtonGroup>
                <button onClick={() => openModal(u)}><FaEdit /></button>
                <Link to={`/admin/users/${u.userId}`} className="button-secondary" style={{ padding: '6px 12px', fontSize: '0.8rem', textDecoration: 'none' }}><FaEye /></Link>
              </ButtonGroup></td>
            </tr>
          ))}
        </tbody>
      </Table>

      {isModalOpen && (
        <ModalOverlay onClick={closeModal}>
          <ModalContent onClick={e => e.stopPropagation()}>
            <h3>Edit User</h3>
            {error && <p style={{ color: 'red' }}>{error}</p>}
            <Field>
              <label>User ID</label>
              <Input value={form.userId} disabled />
            </Field>
            <Field>
              <label>Roles</label>
              <Select isMulti options={roleOptions} value={roleOptions.filter(o => form.roles.includes(o.value))} onChange={selected => setForm(prev => ({ ...prev, roles: selected.map(s => s.value) }))} />
            </Field>
            <Field>
              <label><Checkbox type="checkbox" checked={form.isActive} onChange={e => setForm(prev => ({ ...prev, isActive: e.target.checked }))} /> Active</label>
            </Field>
            <Actions>
              <button onClick={closeModal} className="button-secondary">Cancel</button>
              <button onClick={handleSave} className="button-primary">Save</button>
            </Actions>
          </ModalContent>
        </ModalOverlay>
      )}
    </Container>
  );
};

export default UsersManagement; 