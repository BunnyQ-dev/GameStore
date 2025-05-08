import { useState, useEffect } from 'react';
import { Home, ShoppingBag, ArrowLeft, AlertTriangle } from 'lucide-react';

const MeteorEffect = () => {
  return (
    <div className="meteor-container">
      {Array.from({ length: 5 }).map((_, i) => (
        <div
          key={i}
          className="meteor-element animate-meteor" // animate-meteor is defined in NotFound's style jsx
          style={{
            top: `${10 + i * 15}%`,
            left: '0%', // Initial position, animation handles movement
            width: '150px',
            height: '1px', // Was h-1
            backgroundImage: 'linear-gradient(to right, #3b82f6, transparent)', // Was bg-gradient-to-r from-blue-500 to-transparent
            borderRadius: '9999px', // Was rounded-full
            position: 'absolute',
            animationDelay: `${i * 2.5}s`,
            animationDuration: '6s' // This overrides any duration in animate-meteor class
          }}
        />
      ))}
      <style jsx>{`
        .meteor-container {
          position: absolute;
          top: 0;
          right: 0;
          bottom: 0;
          left: 0;
          overflow: hidden;
          pointer-events: none;
        }
        /* animate-meteor keyframes and class are defined in NotFound component style jsx */
      `}</style>
    </div>
  );
};

const NotFound = () => {
  const [showAnimation, setShowAnimation] = useState(false);
  const [isHovering, setIsHovering] = useState(false);

  useEffect(() => {
    setShowAnimation(true);

    const handleMouseMove = (e) => {
      if (!isHovering) return;

      const planet = document.querySelector('.planet-container-selector'); // Used a more specific selector
      if (planet) {
        const x = (window.innerWidth / 2 - e.pageX) / 30;
        const y = (window.innerHeight / 2 - e.pageY) / 30;
        planet.style.transform = `translateX(${x}px) translateY(${y}px)`;
      }
    };

    window.addEventListener('mousemove', handleMouseMove);
    return () => window.removeEventListener('mousemove', handleMouseMove);
  }, [isHovering]);

  return (
    <div
      className="not-found-page"
      onMouseEnter={() => setIsHovering(true)}
      onMouseLeave={() => setIsHovering(false)}
    >
      {/* Фонові декоративні елементи */}
      <div className="decorative-bg-container">
        <div className="deco-pulse-1" style={{ animationDelay: '0s' }}></div>
        <div className="deco-pulse-2" style={{ animationDelay: '1s' }}></div>
        <div className="deco-pulse-3" style={{ animationDelay: '2s' }}></div>
      </div>

      {/* Анімовані метеори */}
      <MeteorEffect />

      {/* Космічний об'єкт з анімацією зірок */}
      <div className="stars-container">
        {Array.from({ length: 40 }).map((_, i) => (
          <div
            key={i}
            className="star animate-twinkle"
            style={{
              width: Math.random() * 3 + 1 + 'px',
              height: Math.random() * 3 + 1 + 'px',
              top: Math.random() * 100 + '%',
              left: Math.random() * 100 + '%',
              opacity: Math.random() * 0.5 + 0.2, // Initial opacity, animation will change it
              animationDuration: `${Math.random() * 5 + 3}s`, // Overrides duration in animate-twinkle
              animationDelay: `${Math.random() * 3}s`
            }}
          />
        ))}
      </div>

      {/* Контейнер з основним вмістом */}
      <div className={`main-content ${showAnimation ? 'visible' : 'hidden-initial'}`}>
        {/* SVG ілюстрація з планетою */}
        <div className="planet-wrapper planet-container-selector"> {/* Added planet-container-selector for JS */}
          <div className="planet-ping-effect"></div>
          <svg viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg" className="planet-svg">
            <defs>
              <radialGradient id="planetGradient" cx="50%" cy="50%" r="50%" fx="50%" fy="50%">
                <stop offset="0%" stopColor="#0f2942" />
                <stop offset="100%" stopColor="#0a1720" />
              </radialGradient>
              <radialGradient id="glowGradient" cx="50%" cy="50%" r="50%" fx="50%" fy="50%">
                <stop offset="0%" stopColor="#3b82f6" stopOpacity="0.3" />
                <stop offset="100%" stopColor="#3b82f6" stopOpacity="0" />
              </radialGradient>
            </defs>
            <circle cx="100" cy="100" r="90" fill="url(#planetGradient)" />
            <circle cx="100" cy="100" r="100" fill="url(#glowGradient)" />
            <ellipse cx="100" cy="100" rx="85" ry="15" fill="none" stroke="#3b82f6" strokeWidth="1" strokeOpacity="0.3" transform="rotate(30, 100, 100)" />
            <circle cx="70" cy="80" r="5" fill="white" opacity="0.8" />
            <circle cx="125" cy="65" r="3" fill="white" opacity="0.6" />
            <circle cx="135" cy="110" r="4" fill="white" opacity="0.7" />
            <circle cx="85" cy="125" r="2" fill="white" opacity="0.5" />
            <circle cx="60" cy="105" r="3" fill="white" opacity="0.6" />
            <text x="52" y="115" fontFamily="sans-serif" fontWeight="bold" fontSize="45" fill="white" className="text-404">404</text>
          </svg>
        </div>

        {/* Заголовок з анімацією */}
        <div className="title-container">
          <h1 className="main-title animate-pulse-text">404</h1>
          <div className="title-blur-effect animate-pulse-bg"></div>
        </div>

        {/* Підзаголовок з ефектом тіні */}
        <h2 className="subtitle">Page not found</h2>

        {/* Опис з покращеним форматуванням */}
        <div className="description-container">
          <AlertTriangle className="description-icon" size={20} />
          <p className="description-text">
            It seems that the page you are looking for does not exist or has been moved.
            Let's help you get back on the right track.
          </p>
        </div>

        {/* Кнопки з покращеними ефектами */}
        <div className="buttons-container">
          <a href="/" className="button button-home group">
            <Home className="button-icon" size={18} />
            <span>Back to home</span>
          </a>
          <a href="/store" className="button button-store group">
            <ShoppingBag className="button-icon" size={18} />
            <span>Back to store</span>
          </a>
          <button
            onClick={() => window.history.back()}
            className="button button-back group"
          >
            <ArrowLeft className="button-icon" size={18} />
            <span>Back</span>
          </button>
        </div>

        <div className="animated-line-container">
          <div className="animated-line-element animate-slide"></div>
        </div>

        <div className="contact-prompt">
          If you think this is an error, please,
            <a href="#" className="contact-link">contact us</a>
          and we will fix it as soon as possible.
        </div>
      </div>

      <style jsx global>{` // Using global for keyframes to be accessible by MeteorEffect
        body, html {
          margin: 0;
          padding: 0;
          width: 100%;
          height: 100%;
          background-color: #111827; // Default background for page
        }
      `}</style>
      <style jsx>{`
        @keyframes twinkle {
          0%, 100% { opacity: 0.2; } /* Base opacity, inline style can modify this target */
          50% { opacity: 0.8; } /* Base opacity, inline style can modify this target */
        }
        
        @keyframes meteor {
          0% { transform: translateX(-100%) translateY(0); opacity: 1; }
          70% { opacity: 1; }
          100% { transform: translateX(200%) translateY(100px); opacity: 0; }
        }
        
        @keyframes slide {
          0% { transform: translateX(-100%); }
          100% { transform: translateX(100%); }
        }

        @keyframes pulse-kf { /* Renamed from pulse to avoid conflict if any global pulse exists */
          0%, 100% { opacity: 1; }
          50% { opacity: 0.5; }
        }

        @keyframes ping-kf { /* Renamed from ping */
          75%, 100% { transform: scale(2); opacity: 0; }
        }
        
        @keyframes bounce-kf { /* Renamed from bounce */
          0%, 100% { 
            transform: translateY(-25%); 
            animation-timing-function: cubic-bezier(0.8,0,1,1); 
          }
          50% { 
            transform: translateY(0); 
            animation-timing-function: cubic-bezier(0,0,0.2,1); 
          }
        }

        .animate-twinkle {
          animation-name: twinkle;
          animation-timing-function: ease-in-out;
          animation-iteration-count: infinite;
          /* animation-duration is set inline */
        }
        
        .animate-meteor {
          animation-name: meteor;
          animation-timing-function: linear;
          animation-iteration-count: infinite;
          /* animation-duration is set inline */
        }
        
        .animate-slide {
          animation: slide 2s linear infinite;
        }

        /* Page container */
        .not-found-page {
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          text-align: center;
          padding-left: 1rem;
          padding-right: 1rem;
          background-image: linear-gradient(to bottom, #111827, #1f2937, #111827); /* from-gray-900 via-gray-800 to-gray-900 */
          position: relative;
          overflow: hidden;
          color: white; /* Default text color */
        }

        /* Decorative background elements */
        .decorative-bg-container {
          position: absolute;
          top: 0;
          right: 0;
          bottom: 0;
          left: 0;
          overflow: hidden;
          pointer-events: none; /* So they don't interfere with mouse events */
        }
        .deco-pulse-1, .deco-pulse-2, .deco-pulse-3 {
          position: absolute;
          border-radius: 9999px; /* rounded-full */
          opacity: 0.05; /* opacity-5 */
          animation: pulse-kf 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
        }
        .deco-pulse-1 {
          top: 25%;
          left: 25%;
          width: 16rem; /* w-64 */
          height: 16rem; /* h-64 */
          background-color: #3b82f6; /* bg-blue-500 */
        }
        .deco-pulse-2 {
          bottom: 33.33%; /* bottom-1/3 */
          right: 33.33%; /* right-1/3 */
          width: 8rem; /* w-32 */
          height: 8rem; /* h-32 */
          background-color: #a855f7; /* bg-purple-500 */
        }
        .deco-pulse-3 {
          top: 50%;
          right: 25%;
          width: 12rem; /* w-48 */
          height: 12rem; /* h-48 */
          background-color: #6366f1; /* bg-indigo-500 */
        }

        /* Stars container */
        .stars-container {
          position: absolute;
          width: 100%;
          height: 100%;
          overflow: hidden;
          pointer-events: none;
        }
        .star {
          position: absolute;
          background-color: white; /* bg-white */
          border-radius: 9999px; /* rounded-full */
          /* other styles (width, height, top, left, opacity, animation) are inline */
        }

        /* Main content container & visibility animation */
        .main-content {
          position: relative;
          z-index: 10;
          transform: translateY(2rem); /* translate-y-8 */
          opacity: 0; /* opacity-0 */
          transition: transform 700ms, opacity 700ms; /* transition-all duration-700 */
        }
        .main-content.visible {
          transform: translateY(0px); /* translate-y-0 */
          opacity: 1; /* opacity-100 */
        }

        /* Planet SVG illustration */
        .planet-wrapper {
          width: 16rem; /* w-64 */
          height: 16rem; /* h-64 */
          margin-left: auto;
          margin-right: auto;
          margin-bottom: 1.5rem; /* mb-6 */
          position: relative;
          transition: transform 300ms; /* transition-transform duration-300 */
        }
        .planet-ping-effect {
          position: absolute;
          top: 0; right: 0; bottom: 0; left: 0; /* inset-0 */
          background-color: #3b82f6; /* bg-blue-500 */
          border-radius: 9999px; /* rounded-full */
          opacity: 0.1; /* opacity-10 */
          animation: ping-kf 4s cubic-bezier(0, 0, 0.2, 1) infinite; /* animate-ping, adjusted duration from style */
        }
        .planet-svg {
          filter: drop-shadow(0 20px 13px rgb(0 0 0 / 0.03)) drop-shadow(0 8px 5px rgb(0 0 0 / 0.08)); /* Approximation of drop-shadow-2xl */
        }
        .text-404 {
          filter: drop-shadow(0 4px 3px rgb(0 0 0 / 0.07)) drop-shadow(0 2px 2px rgb(0 0 0 / 0.06)); /* drop-shadow-md */
        }

        /* Title */
        .title-container {
          position: relative;
          margin-bottom: 1rem; /* mb-4 */
        }
        .main-title {
          font-size: 6rem; /* text-8xl */
          font-weight: bold; /* font-bold */
          color: transparent; /* text-transparent */
          background-clip: text;
          -webkit-background-clip: text;
          background-image: linear-gradient(to right, #60a5fa, #6366f1, #a855f7); /* bg-gradient-to-r from-blue-400 via-indigo-500 to-purple-500 */
        }
        .animate-pulse-text { /* For the h1 text itself if it needs a separate pulse from its bg */
            animation: pulse-kf 3s cubic-bezier(0.4, 0, 0.6, 1) infinite; /* animationDuration: '3s' */
        }
        .title-blur-effect {
          position: absolute;
          top: -0.25rem; right: -0.25rem; bottom: -0.25rem; left: -0.25rem; /* -inset-1 */
          background-color: #3b82f6; /* bg-blue-500 */
          filter: blur(16px); /* blur-lg */
          opacity: 0.20; /* opacity-20 */
        }
        .animate-pulse-bg { /* For the blur effect */
            animation: pulse-kf 2s cubic-bezier(0.4, 0, 0.6, 1) infinite; /* default pulse */
        }


        /* Subtitle */
        .subtitle {
          font-size: 1.875rem; /* text-3xl */
          font-weight: 600; /* font-semibold */
          color: white; /* text-white */
          margin-bottom: 1.5rem; /* mb-6 */
          text-shadow: 0 4px 6px rgba(0,0,0,0.1), 0 1px 3px rgba(0,0,0,0.08); /* Approximation of drop-shadow-lg */
        }

        /* Description */
        .description-container {
          display: flex;
          align-items: center;
          justify-content: center;
          margin-bottom: 2rem; /* mb-8 */
        }
        .description-icon {
          margin-right: 0.75rem; /* mr-3 */
          color: #f59e0b; /* text-yellow-500 */
        }
        .description-text {
          color: #d1d5db; /* text-gray-300 */
          max-width: 32rem; /* max-w-lg */
          line-height: 1.625; /* leading-relaxed */
        }

        /* Buttons */
        .buttons-container {
          display: flex;
          flex-wrap: wrap;
          gap: 1rem; /* gap-4 */
          justify-content: center;
        }
        .button {
          padding: 0.75rem 1.5rem; /* px-6 py-3 */
          color: white;
          border-radius: 0.5rem; /* rounded-lg */
          transition: all 300ms; /* transition-all duration-300 */
          transform: scale(1); /* Initial scale */
          display: flex;
          align-items: center;
          box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -1px rgba(0,0,0,0.06); /* shadow-lg */
          text-decoration: none; /* For <a> tags */
        }
        .button:hover {
          transform: scale(1.05); /* hover:scale-105 */
        }
        .button-icon {
          margin-right: 0.5rem; /* mr-2 */
        }
        .group:hover .button-icon { /* group-hover:animate-bounce */
          animation: bounce-kf 1s infinite;
        }

        .button-home {
          background-image: linear-gradient(to right, #2563eb, #1d4ed8); /* from-blue-600 to-blue-700 */
        }
        .button-home:hover {
          background-image: linear-gradient(to right, #1d4ed8, #1e40af); /* hover:from-blue-700 hover:to-blue-800 (example next shades) */
          box-shadow: 0 10px 15px -3px rgba(59, 130, 246, 0.4), 0 4px 6px -2px rgba(59, 130, 246, 0.4); /* hover:shadow-blue-500/40 */
        }
        .button-store {
          background-image: linear-gradient(to right, #4f46e5, #9333ea); /* from-indigo-600 to-purple-600 */
        }
        .button-store:hover {
          background-image: linear-gradient(to right, #4338ca, #7e22ce); /* hover:from-indigo-700 hover:to-purple-700 (example next shades) */
          box-shadow: 0 10px 15px -3px rgba(147, 51, 234, 0.4), 0 4px 6px -2px rgba(147, 51, 234, 0.4); /* hover:shadow-purple-500/40 */
        }
        .button-back {
          background-color: #374151; /* bg-gray-700 */
        }
        .button-back:hover {
          background-color: #4b5563; /* hover:bg-gray-600 */
          box-shadow: 0 10px 15px -3px rgba(107, 114, 128, 0.4), 0 4px 6px -2px rgba(107, 114, 128, 0.4); /* hover:shadow-gray-500/40 */
        }
        
        /* Animated line */
        .animated-line-container {
          position: relative;
          width: 8rem; /* w-32 */
          height: 0.25rem; /* h-1 */
          margin-left: auto;
          margin-right: auto;
          margin-top: 3rem; /* mt-12 */
          margin-bottom: 1.5rem; /* mb-6 */
          overflow: hidden;
          border-radius: 9999px; /* rounded-full */
        }
        .animated-line-element {
          position: absolute;
          top: 0; right: 0; bottom: 0; left: 0; /* inset-0 */
          background-image: linear-gradient(to right, #60a5fa, #6366f1, #a855f7); /* from-blue-400 via-indigo-500 to-purple-500 */
          /* animate-slide is applied directly */
        }

        /* Contact prompt */
        .contact-prompt {
          color: #9ca3af; /* text-gray-400 */
          font-size: 0.875rem; /* text-sm */
          max-width: 28rem; /* max-w-md */
          margin-left: auto;
          margin-right: auto;
          background-color: rgba(17, 24, 39, 0.3); /* bg-gray-900/30, using #111827 for gray-900 */
          padding: 1rem; /* p-4 */
          border-radius: 0.5rem; /* rounded-lg */
          backdrop-filter: blur(4px); /* backdrop-blur-sm */
        }
        .contact-link {
          color: #60a5fa; /* text-blue-400 */
          transition: color 300ms; /* transition duration-300 */
          margin-left: 0.25rem; /* mx-1 */
          margin-right: 0.25rem;
          text-decoration-line: underline; /* underline */
          text-decoration-style: dotted; /* decoration-dotted */
          text-underline-offset: 2px; /* underline-offset-2 */
        }
        .contact-link:hover {
          color: #93c5fd; /* hover:text-blue-300 */
        }

      `}</style>
    </div>
  );
};

export default NotFound;