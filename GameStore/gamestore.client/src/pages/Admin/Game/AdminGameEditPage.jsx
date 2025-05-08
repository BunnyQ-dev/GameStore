import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { addNotification } from '../../../store/slices/uiSlice';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import Select from 'react-select';
import styled from 'styled-components';

const EditPageContainer = styled.div`
  padding: 20px;
  max-width: 900px;
  margin: 20px auto;
  background-color: var(--bg-color-secondary);
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
`;

const FormGrid = styled.form`
  display: grid;
  grid-template-columns: 1fr 1fr; 
  gap: 20px 30px; 

  @media (max-width: 768px) {
      grid-template-columns: 1fr; 
  }
`;

const FormGroup = styled.div`
  display: flex;
  flex-direction: column;
  label {
    margin-bottom: 5px;
    font-weight: bold;
    color: var(--text-color-secondary);
  }
`;

const FullWidthGroup = styled(FormGroup)`
  grid-column: 1 / -1; 
`;

const Input = styled.input`
  padding: 10px;
  border: 1px solid var(--bg-color-tertiary);
  background-color: var(--bg-color);
  color: var(--text-color);
  border-radius: 4px;
  width: 100%;
`;

const TextArea = styled.textarea`
  padding: 10px;
  border: 1px solid var(--bg-color-tertiary);
  background-color: var(--bg-color);
  color: var(--text-color);
  border-radius: 4px;
  min-height: 120px;
  width: 100%;
  resize: vertical;
`;

const CheckboxGroup = styled.div`
  display: flex;
  align-items: center;
  label {
    margin-left: 10px;
    margin-bottom: 0;
  }
`;

const ActionsContainer = styled.div`
  grid-column: 1 / -1;
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid var(--bg-color-tertiary);
`;

const selectStyles = {
  control: (provided) => ({
    ...provided,
    backgroundColor: 'var(--bg-color)',
    borderColor: 'var(--bg-color-tertiary)',
    minHeight: '40px',
    boxShadow: 'none',
    '&:hover': {
      borderColor: 'var(--accent-color)'
    }
  }),
  menu: (provided) => ({
    ...provided,
    backgroundColor: 'var(--bg-color-secondary)',
    zIndex: 5 
  }),
  option: (provided, state) => ({
    ...provided,
    backgroundColor: state.isSelected ? 'var(--accent-color)' : state.isFocused ? 'var(--bg-color-tertiary)' : 'var(--bg-color-secondary)',
    color: 'var(--text-color)',
    '&:active': {
      backgroundColor: 'var(--accent-color)'
    }
  }),
  singleValue: (provided) => ({
    ...provided,
    color: 'var(--text-color)'
  }),
  multiValue: (provided) => ({
     ...provided,
     backgroundColor: 'var(--bg-color-tertiary)'
  }),
  multiValueLabel: (provided) => ({
      ...provided,
      color: 'var(--text-color)'
  }),
  multiValueRemove: (provided) => ({
      ...provided,
      color: 'var(--text-color-secondary)',
      '&:hover': {
          backgroundColor: 'var(--accent-color)',
          color: 'white'
      }
  }),
  input: (provided) => ({
      ...provided,
      color: 'var(--text-color)'
  })
};

const AdminGameEditPage = () => {
  const { gameId } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const [game, setGame] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isSaving, setIsSaving] = useState(false);

  const [developers, setDevelopers] = useState([]);
  const [publishers, setPublishers] = useState([]);
  const [genres, setGenres] = useState([]);
  const [platforms, setPlatforms] = useState([]);

  const [formData, setFormData] = useState({
    id: parseInt(gameId, 10),
    title: '',
    description: '',
    price: 0,
    discountPercentage: 0,
    releaseDate: '', 
    isActive: true,
    developerId: null,
    publisherId: null,
    genreIds: [],
    platformIds: [],
    imageUrl: '', 
    screenshotUrls: [], 
    videoUrl: '' 
  });

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setError(null);
      try {
        const [gameRes, devRes, pubRes, genreRes, platRes] = await Promise.all([
          axios.get(`/api/admin/manage/games/${gameId}`),
          axios.get('/api/admin/manage/developers'),
          axios.get('/api/admin/manage/publishers'),
          axios.get('/api/admin/manage/genres'),
          axios.get('/api/admin/manage/platforms')
        ]);

        const gameData = gameRes.data;
        setGame(gameData);

        setFormData({
            id: gameData.id,
            title: gameData.title || '',
            description: gameData.description || '',
            price: gameData.price || 0,
            discountPercentage: gameData.discountPercentage || 0,
            releaseDate: gameData.releaseDate ? new Date(gameData.releaseDate).toISOString().split('T')[0] : '',
            isActive: gameData.isActive,
            developerId: gameData.developerId,
            publisherId: gameData.publisherId,
            genreIds: gameData.genreIds || [],
            platformIds: gameData.platformIds || [],
            imageUrl: gameData.imageUrl || '',
            screenshotUrls: gameData.screenshotUrls || [],
            videoUrl: gameData.videoUrl || ''
        });

        setDevelopers(devRes.data.map(d => ({ value: d.id, label: d.name })) || []);
        setPublishers(pubRes.data.map(p => ({ value: p.id, label: p.name })) || []);
        setGenres(genreRes.data.map(g => ({ value: g.id, label: g.name })) || []);
        setPlatforms(platRes.data.map(p => ({ value: p.id, label: p.name })) || []);

      } catch (err) {
        console.error("Error fetching data:", err);
        setError('Error fetching data');
        dispatch(addNotification({ message: 'Error fetching data', type: 'error' }));
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [gameId, dispatch]);

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    if (name === 'screenshotUrls') {
      setFormData(prev => ({
        ...prev,
        screenshotUrls: value.split('\n').map(line => line.trim()).filter(line => line)
      }));
      return;
    }
    if (name === 'videoUrl') {
      setFormData(prev => ({ ...prev, videoUrl: value }));
      return;
    }
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSelectChange = (name, selectedOption) => {
    if (Array.isArray(selectedOption)) {
        setFormData(prev => ({ ...prev, [name]: selectedOption.map(opt => opt.value) }));
    } else {
        setFormData(prev => ({ ...prev, [name]: selectedOption ? selectedOption.value : null }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const dataToSend = {
        ...formData,
        price: parseFloat(formData.price) || 0,
        discountPercentage: parseInt(formData.discountPercentage, 10) || 0,
        releaseDate: formData.releaseDate ? new Date(formData.releaseDate).toISOString() : new Date().toISOString()
    };

    try {
      await axios.put(`/api/admin/manage/games/${gameId}`, dataToSend);
      dispatch(addNotification({ message: 'Гру успішно оновлено', type: 'success' }));
      navigate('/admin'); 
    } catch (err) {
      console.error("Error updating game:", err);
      const errorMsg = err.response?.data?.message || 'Error updating game.';
      setError(errorMsg);
      dispatch(addNotification({ message: errorMsg, type: 'error' }));
      setIsSaving(false);
    }
  };

  if (loading) return <LoadingSpinner fullScreen />;
  if (error && !game) return <EditPageContainer><h2>Error</h2><p>{error}</p></EditPageContainer>;
  if (!game) return null; 

  return (
    <EditPageContainer>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h1>Edit Game: {game.title}</h1>
        <Link to="/admin?tab=games">Back to Games List</Link> {} 
      </div>

      {error && <p style={{ color: 'red', marginBottom: '15px' }}>{error}</p>}

      <FormGrid onSubmit={handleSubmit}>
        <FormGroup>
          <label htmlFor="title">Title</label>
          <Input type="text" id="title" name="title" value={formData.title} onChange={handleInputChange} required />
        </FormGroup>

        <FormGroup>
          <label htmlFor="price">Price</label>
          <Input type="number" id="price" name="price" value={formData.price} onChange={handleInputChange} required min="0" step="0.01" />
        </FormGroup>

        <FullWidthGroup>
          <label htmlFor="description">Description</label>
          <TextArea id="description" name="description" value={formData.description} onChange={handleInputChange} required />
        </FullWidthGroup>

        <FormGroup>
            <label htmlFor="developerId">Developer</label>
            <Select
                id="developerId"
                name="developerId"
                options={developers}
                styles={selectStyles}
                value={developers.find(opt => opt.value === formData.developerId) || null}
                onChange={(selected) => handleSelectChange('developerId', selected)}
                isClearable
                placeholder="Select Developer..."
            />
        </FormGroup>

        <FormGroup>
            <label htmlFor="publisherId">Publisher</label>
            <Select
                id="publisherId"
                name="publisherId"
                options={publishers}
                styles={selectStyles}
                value={publishers.find(opt => opt.value === formData.publisherId) || null}
                onChange={(selected) => handleSelectChange('publisherId', selected)}
                isClearable
                placeholder="Select Publisher..."
            />
        </FormGroup>

         <FullWidthGroup>
            <label htmlFor="genreIds">Genres</label>
            <Select
                id="genreIds"
                name="genreIds"
                isMulti
                options={genres}
                styles={selectStyles}
                value={genres.filter(opt => formData.genreIds.includes(opt.value))}
                onChange={(selected) => handleSelectChange('genreIds', selected)}
                placeholder="Select Genres..."
            />
        </FullWidthGroup>

         <FullWidthGroup>
            <label htmlFor="platformIds">Platforms</label>
            <Select
                id="platformIds"
                name="platformIds"
                isMulti
                options={platforms}
                styles={selectStyles}
                value={platforms.filter(opt => formData.platformIds.includes(opt.value))}
                onChange={(selected) => handleSelectChange('platformIds', selected)}
                placeholder="Select Platforms..."
            />
        </FullWidthGroup>

         <FormGroup>
          <label htmlFor="releaseDate">Release Date</label>
          <Input type="date" id="releaseDate" name="releaseDate" value={formData.releaseDate} onChange={handleInputChange} required />
        </FormGroup>

         <FormGroup>
          <label htmlFor="discountPercentage">Discount (%)</label>
          <Input type="number" id="discountPercentage" name="discountPercentage" value={formData.discountPercentage} onChange={handleInputChange} min="0" max="100" />
        </FormGroup>

        <FullWidthGroup style={{ flexDirection: 'row', alignItems: 'center' }}>
            <label htmlFor="isActive" style={{marginBottom: 0, marginRight: '15px'}}>Is Active?</label>
            <CheckboxGroup>
                 <Input
                    type="checkbox"
                    id="isActive"
                    name="isActive"
                    checked={formData.isActive}
                    onChange={handleInputChange}
                    style={{width: 'auto'}}
                 />
            </CheckboxGroup>
        </FullWidthGroup>

        <FullWidthGroup>
          <label htmlFor="imageUrl">Main Image URL</label>
          <Input
            type="text"
            id="imageUrl"
            name="imageUrl"
            value={formData.imageUrl}
            onChange={handleInputChange}
            placeholder="https://..."
          />
        </FullWidthGroup>

        <FullWidthGroup>
          <label htmlFor="screenshotUrls">Screenshot URLs (one per line)</label>
          <TextArea
            id="screenshotUrls"
            name="screenshotUrls"
            value={formData.screenshotUrls.join('\n')}
            onChange={handleInputChange}
            placeholder="Enter one URL per line"
          />
        </FullWidthGroup>

        <FullWidthGroup>
          <label htmlFor="videoUrl">Video URL (Trailer)</label>
          <Input
            type="text"
            id="videoUrl"
            name="videoUrl"
            value={formData.videoUrl}
            onChange={handleInputChange}
            placeholder="https://..."
          />
        </FullWidthGroup>

        <ActionsContainer>
            <button type="button" onClick={() => navigate('/admin')} className="button-secondary" disabled={isSaving}>
                Cancel
            </button>
            <button type="submit" className="button-primary" disabled={isSaving}>
                {isSaving ? <LoadingSpinner size="small" /> : 'Save Changes'}
            </button>
        </ActionsContainer>

      </FormGrid>
    </EditPageContainer>
  );
};

export default AdminGameEditPage; 