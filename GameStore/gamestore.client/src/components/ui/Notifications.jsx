import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import styled, { keyframes, css } from 'styled-components';
import { 
  FaCheckCircle, 
  FaInfoCircle, 
  FaExclamationTriangle, 
  FaTimesCircle, 
  FaTimes 
} from 'react-icons/fa';

import { removeNotification } from '../../store/slices/uiSlice';

const slideIn = keyframes`
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
`;

const slideOut = keyframes`
  from {
    transform: translateX(0);
    opacity: 1;
  }
  to {
    transform: translateX(100%);
    opacity: 0;
  }
`;

const NotificationsContainer = styled.div`
  position: fixed;
  top: 20px;
  right: 20px;
  width: 300px;
  max-width: calc(100vw - 40px);
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 10px;
`;

const NotificationItem = styled.div`
  display: flex;
  align-items: center;
  padding: 12px 15px;
  border-radius: 4px;
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.2);
  background-color: var(--bg-color-secondary);
  color: var(--text-color);
  animation: ${slideIn} 0.3s ease forwards;
  position: relative;
  overflow: hidden;

  ${props => props.$isExiting && css`
    animation: ${slideOut} 0.3s ease forwards;
  `}

  ${props => {
    switch(props.$type) {
      case 'success':
        return css`
          border-left: 5px solid #4caf50;
          & svg { color: #4caf50; }
        `;
      case 'info':
        return css`
          border-left: 5px solid #2196f3;
          & svg { color: #2196f3; }
        `;
      case 'warning':
        return css`
          border-left: 5px solid #ff9800;
          & svg { color: #ff9800; }
        `;
      case 'error':
        return css`
          border-left: 5px solid #f44336;
          & svg { color: #f44336; }
        `;
      default:
        return '';
    }
  }}
`;

const IconContainer = styled.div`
  display: flex;
  align-items: center;
  margin-right: 12px;
  font-size: 20px;
`;

const Message = styled.p`
  margin: 0;
  padding: 0;
  flex: 1;
  font-size: 14px;
`;

const CloseButton = styled.button`
  background: transparent;
  border: none;
  color: var(--text-color-secondary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  margin-left: 10px;
  transition: color 0.2s;

  &:hover {
    color: var(--text-color);
  }
`;

const Progress = styled.div`
  position: absolute;
  bottom: 0;
  left: 0;
  height: 3px;
  background-color: rgba(0, 0, 0, 0.1);
  width: 100%;

  &::after {
    content: '';
    position: absolute;
    left: 0;
    top: 0;
    height: 100%;
    width: ${props => props.$progress}%;
    background-color: ${props => {
      switch(props.$type) {
        case 'success': return '#4caf50';
        case 'info': return '#2196f3';
        case 'warning': return '#ff9800';
        case 'error': return '#f44336';
        default: return '#2196f3';
      }
    }};
    transition: width 0.1s linear;
  }
`;

const getIcon = (type) => {
  switch(type) {
    case 'success': return <FaCheckCircle />;
    case 'info': return <FaInfoCircle />;
    case 'warning': return <FaExclamationTriangle />;
    case 'error': return <FaTimesCircle />;
    default: return <FaInfoCircle />;
  }
};

const Notifications = () => {
  const dispatch = useDispatch();
  const { notifications } = useSelector(state => state.ui);

  useEffect(() => {
    const timers = notifications.map(notification => {
      if (!notification.isPersistent) {
        return setTimeout(() => {
          dispatch(removeNotification(notification.id));
        }, notification.duration || 5000);
      }
      return null;
    });

    return () => {
      timers.forEach(timer => timer && clearTimeout(timer));
    };
  }, [notifications, dispatch]);

  const handleClose = (id) => {
    dispatch(removeNotification(id));
  };

  if (notifications.length === 0) return null;

  return (
    <NotificationsContainer>
      {notifications.map(notification => {
        const timeLeft = notification.duration 
          ? (notification.createdAt + notification.duration - Date.now()) / notification.duration * 100
          : (notification.createdAt + 5000 - Date.now()) / 5000 * 100;
        
        const progress = Math.max(0, Math.min(100, timeLeft));
        
        return (
          <NotificationItem 
            key={notification.id} 
            $type={notification.type || 'info'}
            $isExiting={progress <= 0}
          >
            <IconContainer>
              {getIcon(notification.type)}
            </IconContainer>
            <Message>{notification.message}</Message>
            <CloseButton onClick={() => handleClose(notification.id)}>
              <FaTimes />
            </CloseButton>
            {!notification.isPersistent && (
              <Progress 
                $type={notification.type || 'info'} 
                $progress={progress}
              />
            )}
          </NotificationItem>
        );
      })}
    </NotificationsContainer>
  );
};

export default Notifications; 