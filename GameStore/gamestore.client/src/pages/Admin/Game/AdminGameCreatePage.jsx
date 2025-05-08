import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { addNotification } from '../../../store/slices/uiSlice';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import Select from 'react-select';
import styled from 'styled-components';

const CreatePageContainer = styled.div`
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
  @media (max-width: 768px) { grid-template-columns: 1fr; }
`;
const FormGroup = styled.div`
  display: flex; flex-direction: column;
  label { margin-bottom: 5px; font-weight: bold; color: var(--text-color-secondary); }
`;
const FullWidthGroup = styled(FormGroup)`grid-column: 1 / -1;`;
const Input = styled.input`
  padding: 10px; border: 1px solid var(--bg-color-tertiary); background: var(--bg-color); color: var(--text-color); border-radius: 4px; width: 100%;
`;
const TextArea = styled.textarea`
  padding: 10px; border: 1px solid var(--bg-color-tertiary); background: var(--bg-color); color: var(--text-color); border-radius: 4px; min-height: 120px; width: 100%; resize: vertical;
`;
const CheckboxGroup = styled.div`
  display: flex; align-items: center;
  label { margin-left: 10px; margin-bottom: 0; }
`;
const ActionsContainer = styled.div`
  grid-column: 1 / -1; display: flex; justify-content: flex-end; gap: 15px; margin-top: 20px; padding-top: 20px; border-top: 1px solid var(--bg-color-tertiary);
`;

const selectStyles = {
  control: provided => ({ ...provided, backgroundColor: 'var(--bg-color)', borderColor: 'var(--bg-color-tertiary)', minHeight: '40px', boxShadow: 'none', '&:hover': { borderColor: 'var(--accent-color)' }}),
  menu: provided => ({ ...provided, backgroundColor: 'var(--bg-color-secondary)', zIndex: 5 }),
  option: (provided, state) => ({ ...provided, backgroundColor: state.isSelected ? 'var(--accent-color)' : state.isFocused ? 'var(--bg-color-tertiary)' : 'var(--bg-color-secondary)', color: 'var(--text-color)', '&:active': { backgroundColor: 'var(--accent-color)' }}),
  singleValue: provided => ({ ...provided, color: 'var(--text-color)' }),
  multiValue: provided => ({ ...provided, backgroundColor: 'var(--bg-color-tertiary)' }),
  multiValueLabel: provided => ({ ...provided, color: 'var(--text-color)' }),
  multiValueRemove: provided => ({ ...provided, color: 'var(--text-color-secondary)', '&:hover': { backgroundColor: 'var(--accent-color)', color: 'white' }}),
  input: provided => ({ ...provided, color: 'var(--text-color)' })
};

const AdminGameCreatePage = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [developers, setDevelopers] = useState([]);
  const [publishers, setPublishers] = useState([]);
  const [genres, setGenres] = useState([]);
  const [platforms, setPlatforms] = useState([]);
  const [formData, setFormData] = useState({
    title: '', description: '', price: 0, discountPercentage: 0,
    releaseDate: '', isActive: true, isFeatured: false,
    developerId: null, publisherId: null,
    genreIds: [], platformIds: [],
    imageUrl: '', backgroundImageUrl: '', videoUrl: '',
    minimumSystemRequirements: '', recommendedSystemRequirements: '',
    screenshotUrls: []
  });

  useEffect(() => {
    const fetchRefs = async () => {
      setLoading(true);
      try {
        const [devRes, pubRes, genreRes, platRes] = await Promise.all([
          axios.get('/api/admin/manage/developers'),
          axios.get('/api/admin/manage/publishers'),
          axios.get('/api/admin/manage/genres'),
          axios.get('/api/admin/manage/platforms')
        ]);
        setDevelopers(devRes.data.map(d => ({ value: d.id, label: d.name })));
        setPublishers(pubRes.data.map(p => ({ value: p.id, label: p.name })));
        setGenres(genreRes.data.map(g => ({ value: g.id, label: g.name })));
        setPlatforms(platRes.data.map(p => ({ value: p.id, label: p.name })));
      } catch (err) {
        console.error(err);
        dispatch(addNotification({ message: 'Помилка завантаження довідників', type: 'error' }));
      } finally { setLoading(false); }
    };
    fetchRefs();
  }, [dispatch]);

  const handleInputChange = e => {
    const { name, value, type, checked } = e.target;
    if (name === 'screenshotUrls') {
      setFormData(prev => ({ ...prev, screenshotUrls: value.split('\n').map(l => l.trim()).filter(l => l) }));
      return;
    }
    setFormData(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
  };

  const handleSelectChange = (name, selected) => {
    if (Array.isArray(selected)) setFormData(prev => ({ ...prev, [name]: selected.map(o => o.value) }));
    else setFormData(prev => ({ ...prev, [name]: selected ? selected.value : null }));
  };

  const handleSubmit = async e => {
    e.preventDefault();
    setError(null);
    if (!formData.developerId) {
      const msg = 'Виберіть розробника';
      dispatch(addNotification({ message: msg, type: 'error' }));
      return;
    }
    if (!formData.publisherId) {
      const msg = 'Виберіть видавця';
      dispatch(addNotification({ message: msg, type: 'error' }));
      return;
    }
    const payload = {
      Title: formData.title,
      Description: formData.description,
      Price: parseFloat(formData.price) || 0,
      ReleaseDate: formData.releaseDate ? new Date(formData.releaseDate).toISOString() : new Date().toISOString(),
      DeveloperId: formData.developerId,
      PublisherId: formData.publisherId,
      ImageUrl: formData.imageUrl || '',
      VideoUrl: formData.videoUrl || '',
      CoverImageUrl: formData.imageUrl || '',
      BackgroundImageUrl: formData.backgroundImageUrl || '',
      TrailerUrl: formData.videoUrl || '',
      MinimumSystemRequirements: formData.minimumSystemRequirements || '',
      RecommendedSystemRequirements: formData.recommendedSystemRequirements || '',
      DiscountPercentage: parseInt(formData.discountPercentage, 10) || 0,
      IsFeatured: formData.isFeatured,
      GenreIds: formData.genreIds,
      PlatformIds: formData.platformIds,
      Screenshots: formData.screenshotUrls
    };
    try {
      const res = await axios.post('/api/admin/games', payload);
      const newId = res.data.id || res.data.Id;
      dispatch(addNotification({ message: 'Гру успішно створено', type: 'success' }));
      navigate(`/admin/games/edit/${newId}`);
    } catch (err) {
      console.error(err);
      const msg = err.response?.data?.message || 'Помилка створення гри.';
      setError(msg);
      dispatch(addNotification({ message: msg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner fullScreen />;

  return (
    <CreatePageContainer>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h1>Create New Game</h1>
        <Link to="/admin?tab=games">Back to Games List</Link>
      </div>
      {error && <p style={{ color: 'red', marginBottom: '15px' }}>{error}</p>}
      <FormGrid onSubmit={handleSubmit}>
        {/* Title, Price, Description, Developer, Publisher, Genres, Platforms, ReleaseDate, Discount, IsActive, IsFeatured, ImageUrl, BackgroundImageUrl, VideoUrl, MinSys, RecSys, ScreenshotUrls */}
        <FormGroup><label>Title</label><Input name="title" value={formData.title} onChange={handleInputChange} required /></FormGroup>
        <FormGroup><label>Price</label><Input name="price" type="number" step="0.01" value={formData.price} onChange={handleInputChange} required /></FormGroup>
        <FullWidthGroup><label>Description</label><TextArea name="description" value={formData.description} onChange={handleInputChange} /></FullWidthGroup>
        <FormGroup><label>Developer</label><Select name="developerId" options={developers} styles={selectStyles} value={developers.find(o => o.value === formData.developerId) || null} onChange={s => handleSelectChange('developerId', s)} isClearable /></FormGroup>
        <FormGroup><label>Publisher</label><Select name="publisherId" options={publishers} styles={selectStyles} value={publishers.find(o => o.value === formData.publisherId) || null} onChange={s => handleSelectChange('publisherId', s)} isClearable /></FormGroup>
        <FullWidthGroup><label>Genres</label><Select name="genreIds" options={genres} styles={selectStyles} isMulti value={genres.filter(o => formData.genreIds.includes(o.value))} onChange={s => handleSelectChange('genreIds', s)} /></FullWidthGroup>
        <FullWidthGroup><label>Platforms</label><Select name="platformIds" options={platforms} styles={selectStyles} isMulti value={platforms.filter(o => formData.platformIds.includes(o.value))} onChange={s => handleSelectChange('platformIds', s)} /></FullWidthGroup>
        {/* Additional fields... */}
        <FormGroup><label>Release Date</label><Input name="releaseDate" type="date" value={formData.releaseDate} onChange={handleInputChange} /></FormGroup>
        <FormGroup><label>Discount (%)</label><Input name="discountPercentage" type="number" value={formData.discountPercentage} onChange={handleInputChange} /></FormGroup>
        <FormGroup><label><CheckboxGroup><Input name="isActive" type="checkbox" checked={formData.isActive} onChange={handleInputChange} /> Active</CheckboxGroup></label></FormGroup>
        <FormGroup><label><CheckboxGroup><Input name="isFeatured" type="checkbox" checked={formData.isFeatured} onChange={handleInputChange} /> Featured</CheckboxGroup></label></FormGroup>
        <FullWidthGroup><label>Main Image URL</label><Input name="imageUrl" value={formData.imageUrl} onChange={handleInputChange} /></FullWidthGroup>
        <FullWidthGroup><label>Background Image URL</label><Input name="backgroundImageUrl" value={formData.backgroundImageUrl} onChange={handleInputChange} /></FullWidthGroup>
        <FullWidthGroup><label>Video URL</label><Input name="videoUrl" value={formData.videoUrl} onChange={handleInputChange} /></FullWidthGroup>
        <FullWidthGroup><label>Min System Requirements</label><TextArea name="minimumSystemRequirements" value={formData.minimumSystemRequirements} onChange={handleInputChange} /></FullWidthGroup>
        <FullWidthGroup><label>Recommended System Requirements</label><TextArea name="recommendedSystemRequirements" value={formData.recommendedSystemRequirements} onChange={handleInputChange} /></FullWidthGroup>
        <FullWidthGroup><label>Screenshot URLs (one per line)</label><TextArea name="screenshotUrls" value={formData.screenshotUrls.join('\n')} onChange={handleInputChange} /></FullWidthGroup>
        <ActionsContainer>
          <button type="button" onClick={() => navigate('/admin?tab=games')} className="button-secondary">Cancel</button>
          <button type="submit" className="button-primary">Create Game</button>
        </ActionsContainer>
      </FormGrid>
    </CreatePageContainer>
  );
};

export default AdminGameCreatePage; 