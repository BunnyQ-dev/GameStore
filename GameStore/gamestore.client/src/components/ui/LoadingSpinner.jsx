import styled, { keyframes } from 'styled-components';

const spin = keyframes`
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
`;

const Overlay = styled.div`
  position: ${props => props.$fullScreen ? 'fixed' : 'absolute'};
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: ${props => props.$fullScreen ? 'var(--bg-color)' : 'rgba(23, 26, 33, 0.75)'};
  z-index: 9999;
`;

const Spinner = styled.div`
  border: 4px solid rgba(102, 192, 244, 0.2);
  border-top: 4px solid var(--accent-color);
  border-radius: 50%;
  width: ${props => props.$size || '40px'};
  height: ${props => props.$size || '40px'};
  animation: ${spin} 1s linear infinite;
`;

const SpinnerContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
`;

const LoadingText = styled.div`
  color: var(--text-color);
  font-size: 1rem;
  margin-top: 0.5rem;
`;

const LoadingSpinner = ({ 
  fullScreen = false, 
  size = '40px', 
  text = 'Loading...',
  showText = true 
}) => {
  if (fullScreen || size === 'large') {
    return (
      <Overlay $fullScreen={fullScreen}>
        <SpinnerContainer>
          <Spinner $size={size === 'large' ? '80px' : size} />
          {showText && <LoadingText>{text}</LoadingText>}
        </SpinnerContainer>
      </Overlay>
    );
  }

  return (
    <SpinnerContainer>
      <Spinner $size={size} />
      {showText && <LoadingText>{text}</LoadingText>}
    </SpinnerContainer>
  );
};

export default LoadingSpinner; 