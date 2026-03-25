import React from 'react';

const statusColors = {
  Submitted: { bg: '#e3f2fd', color: '#1565c0' },
  UnderReview: { bg: '#fff3e0', color: '#e65100' },
  DocumentsRequested: { bg: '#fce4ec', color: '#c62828' },
  UnderInvestigation: { bg: '#f3e5f5', color: '#6a1b9a' },
  Approved: { bg: '#e8f5e9', color: '#2e7d32' },
  Rejected: { bg: '#ffebee', color: '#b71c1c' },
  PaymentScheduled: { bg: '#e0f7fa', color: '#00695c' },
  PaymentCompleted: { bg: '#e8f5e9', color: '#1b5e20' },
  Closed: { bg: '#eceff1', color: '#37474f' },
  Appealed: { bg: '#fff8e1', color: '#f57f17' },
};

function StatusBadge({ status }) {
  const style = statusColors[status] || { bg: '#eceff1', color: '#546e7a' };

  const displayName = status
    ? status.replace(/([A-Z])/g, ' $1').trim()
    : 'Unknown';

  return (
    <span
      className="status-badge"
      style={{
        backgroundColor: style.bg,
        color: style.color,
        padding: '4px 12px',
        borderRadius: '12px',
        fontSize: '0.8rem',
        fontWeight: 600,
        display: 'inline-block',
      }}
    >
      {displayName}
    </span>
  );
}

export default StatusBadge;
