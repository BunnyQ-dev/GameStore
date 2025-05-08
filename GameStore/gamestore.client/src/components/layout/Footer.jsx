import { Link } from 'react-router-dom';
import styled from 'styled-components';
import { FaGithub, FaTwitter, FaDiscord, FaFacebook, FaInstagram } from 'react-icons/fa';

const FooterContainer = styled.footer`
  background-color: var(--bg-color);
  color: var(--text-color-secondary);
  padding: 2rem 0;
  margin-top: auto;
  border-top: 1px solid var(--bg-color-tertiary);
`;

const FooterContent = styled.div`
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  padding: 0 1rem;
`;

const FooterSection = styled.div`
  display: flex;
  flex-direction: column;
`;

const FooterTitle = styled.h3`
  color: var(--text-color);
  margin-bottom: 1rem;
  font-size: 1.2rem;
`;

const FooterLink = styled(Link)`
  color: var(--text-color-secondary);
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
  
  &:hover {
    color: var(--accent-color);
  }
`;

const SocialLinks = styled.div`
  display: flex;
  gap: 1rem;
  margin-top: 0.5rem;
`;

const SocialIcon = styled.a`
  color: var(--text-color-secondary);
  font-size: 1.5rem;
  
  &:hover {
    color: var(--accent-color);
  }
`;

const BottomBar = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid var(--bg-color-tertiary);
  text-align: center;
`;

const Copyright = styled.p`
  color: var(--text-color-secondary);
  font-size: 0.8rem;
  margin-bottom: 0.5rem;
`;

const ValveLogo = styled.img`
  height: 30px;
  margin-top: 0.5rem;
  opacity: 0.7;
`;

const Footer = () => {
  const currentYear = new Date().getFullYear();
  
  return (
    <FooterContainer>
      <FooterContent>
        <FooterSection>
          <FooterTitle>Store</FooterTitle>
          <FooterLink to="/store">Home</FooterLink>
          <FooterLink to="#">Categories</FooterLink>
          <FooterLink to="#">New Releases</FooterLink>
          <FooterLink to="#">Top Sellers</FooterLink>
          <FooterLink to="#">Special Offers</FooterLink>
        </FooterSection>
        
        <FooterSection>
          <FooterTitle>About Us</FooterTitle>
          <FooterLink to="#">About GameStore</FooterLink>
          <FooterLink to="#">Careers</FooterLink>
          <FooterLink to="#">Press</FooterLink>
          <FooterLink to="#">Privacy Policy</FooterLink>
          <FooterLink to="#">Legal Information</FooterLink>
        </FooterSection>
        
        <FooterSection>
          <FooterTitle>Support</FooterTitle>
          <FooterLink to="#">Support Center</FooterLink>
          <FooterLink to="#">FAQ</FooterLink>
          <FooterLink to="#">Contact Us</FooterLink>
          <FooterLink to="#">Refund Policy</FooterLink>
        </FooterSection>
        
        <FooterSection>
          <FooterTitle>Community</FooterTitle>
          <FooterLink to="#">Community Hub</FooterLink>
          <FooterLink to="#">Forums</FooterLink>
          <FooterLink to="#">Workshops</FooterLink>
          <SocialLinks>
            <SocialIcon href="#" target="_blank" rel="noopener noreferrer">
              <FaGithub />
            </SocialIcon>
            <SocialIcon href="#" target="_blank" rel="noopener noreferrer">
              <FaTwitter />
            </SocialIcon>
            <SocialIcon href="#" target="_blank" rel="noopener noreferrer">
              <FaDiscord />
            </SocialIcon>
            <SocialIcon href="#" target="_blank" rel="noopener noreferrer">
              <FaFacebook />
            </SocialIcon>
            <SocialIcon href="#" target="_blank" rel="noopener noreferrer">
              <FaInstagram />
            </SocialIcon>
          </SocialLinks>
        </FooterSection>
      </FooterContent>
      
      <BottomBar>
        <Copyright>
          © {currentYear} GameStore Corporation. All rights reserved. All trademarks are property of their respective owners.
        </Copyright>
      </BottomBar>
    </FooterContainer>
  );
};

export default Footer; 