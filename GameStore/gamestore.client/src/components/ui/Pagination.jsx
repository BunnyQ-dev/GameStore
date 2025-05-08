import React from 'react';
import './Pagination.css'; 

const Pagination = ({ currentPage, totalPages, onPageChange }) => {
  if (totalPages <= 1) {
    return null; 
  }

  const handlePrevious = () => {
    if (currentPage > 1) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNext = () => {
    if (currentPage < totalPages) {
      onPageChange(currentPage + 1);
    }
  };

  return (
    <div className="pagination">
      <button onClick={handlePrevious} disabled={currentPage === 1}>
        <i className="fa fa-chevron-left"></i> Previous
      </button>
      <span>
        Page {currentPage} of {totalPages}
      </span>
      <button onClick={handleNext} disabled={currentPage === totalPages}>
        Next <i className="fa fa-chevron-right"></i>
      </button>
    </div>
  );
};

export default Pagination; 