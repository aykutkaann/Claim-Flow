import React, { useState, useEffect } from 'react';
import api from '../api';
import StatusBadge from '../components/StatusBadge';

const triggers = [
  { value: 0, label: 'Start Review', btn: 'btn-primary' },
  { value: 1, label: 'Request Documents', btn: 'btn-warning' },
  { value: 2, label: 'Start Investigation', btn: 'btn-secondary' },
  { value: 3, label: 'Approve', btn: 'btn-success' },
  { value: 4, label: 'Reject', btn: 'btn-danger' },
  { value: 5, label: 'Schedule Payment', btn: 'btn-primary' },
  { value: 6, label: 'Confirm Payment', btn: 'btn-success' },
  { value: 7, label: 'Close', btn: 'btn-secondary' },
  { value: 8, label: 'File Appeal', btn: 'btn-warning' },
];

// Map status to valid transitions
const statusTransitions = {
  Submitted: [0], // StartReview
  UnderReview: [1, 2, 3, 4], // RequestDocuments, StartInvestigation, Approve, Reject
  DocumentsRequested: [0], // StartReview (back to review)
  UnderInvestigation: [3, 4], // Approve, Reject
  Approved: [5], // SchedulePayment
  Rejected: [8], // FileAppeal
  PaymentScheduled: [6], // ConfirmPayment
  PaymentCompleted: [7], // Close
  Appealed: [0], // StartReview
};

function Claims() {
  const [view, setView] = useState('list'); // 'list', 'create', 'detail'
  const [policies, setPolicies] = useState([]);
  const [claimId, setClaimId] = useState('');
  const [searchId, setSearchId] = useState('');
  const [claim, setClaim] = useState(null);
  const [fraudResult, setFraudResult] = useState(null);
  const [form, setForm] = useState({
    policyId: '',
    description: '',
    claimedAmount: '',
  });
  const [transitionForm, setTransitionForm] = useState({
    changedBy: '',
    notes: '',
  });
  const [message, setMessage] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadPolicies();
  }, []);

  async function loadPolicies() {
    try {
      const res = await api.get('/api/policies');
      setPolicies(res.data);
    } catch (err) {
      // silently fail
    }
  }

  async function handleCreateClaim(e) {
    e.preventDefault();
    setMessage(null);
    try {
      const res = await api.post('/api/claims', {
        policyId: form.policyId,
        description: form.description,
        claimedAmount: Number(form.claimedAmount),
      });
      const newClaimId = res.data.id || res.data;
      setMessage({
        type: 'success',
        text: `Claim created! ID: ${newClaimId}`,
      });
      setForm({ policyId: '', description: '', claimedAmount: '' });
      if (newClaimId) {
        setClaimId(newClaimId);
        loadClaim(newClaimId);
      }
    } catch (err) {
      const errorMsg =
        err.response?.data?.title || err.response?.data || 'Failed to create claim.';
      setMessage({ type: 'error', text: String(errorMsg) });
    }
  }

  async function loadClaim(id) {
    if (!id) return;
    setLoading(true);
    setMessage(null);
    setFraudResult(null);
    try {
      const res = await api.get(`/api/claims/${id}`);
      setClaim(res.data);
      setClaimId(id);
      setView('detail');
    } catch (err) {
      setMessage({ type: 'error', text: 'Claim not found.' });
      setClaim(null);
    }
    setLoading(false);
  }

  async function handleTransition(triggerValue) {
    if (!transitionForm.changedBy) {
      setMessage({ type: 'error', text: 'Please enter "Changed By" name.' });
      return;
    }
    setMessage(null);
    try {
      await api.patch(`/api/claims/${claimId}/transition`, {
        trigger: triggerValue,
        changedBy: transitionForm.changedBy,
        notes: transitionForm.notes,
      });
      setMessage({ type: 'success', text: 'Transition successful!' });
      setTransitionForm({ ...transitionForm, notes: '' });
      loadClaim(claimId);
    } catch (err) {
      const errorMsg =
        err.response?.data?.title ||
        err.response?.data?.detail ||
        err.response?.data ||
        'Transition failed.';
      setMessage({ type: 'error', text: String(errorMsg) });
    }
  }

  async function handleFraudCheck() {
    setMessage(null);
    try {
      const res = await api.get(`/api/claims/${claimId}/fraud-check`);
      setFraudResult(res.data);
    } catch (err) {
      setMessage({ type: 'error', text: 'Fraud check failed.' });
    }
  }

  function getFraudScoreClass(score) {
    if (score < 30) return 'low';
    if (score <= 60) return 'medium';
    return 'high';
  }

  function getValidTransitions() {
    const status = claim?.currentStatus || claim?.status;
    return statusTransitions[status] || [];
  }

  // LIST VIEW
  if (view === 'list') {
    return (
      <div>
        <div className="page-header">
          <h2>Claims</h2>
          <p>Search, create, and manage insurance claims</p>
        </div>

        {message && (
          <div className={`message ${message.type}`}>{message.text}</div>
        )}

        <div className="card">
          <h3>Look Up Claim</h3>
          <div className="search-bar">
            <input
              type="text"
              value={searchId}
              onChange={(e) => setSearchId(e.target.value)}
              placeholder="Enter claim ID..."
            />
            <button
              className="btn btn-primary"
              onClick={() => loadClaim(searchId)}
            >
              Search
            </button>
          </div>
        </div>

        <div className="card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
            <h3 style={{ margin: 0 }}>Create New Claim</h3>
          </div>
          <form onSubmit={handleCreateClaim}>
            <div className="form-grid">
              <div className="form-group">
                <label>Policy</label>
                <select
                  value={form.policyId}
                  onChange={(e) =>
                    setForm({ ...form, policyId: e.target.value })
                  }
                  required
                >
                  <option value="">Select a policy...</option>
                  {policies.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.policyNumber} ({p.id?.substring(0, 8)}...)
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Claimed Amount ($)</label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  value={form.claimedAmount}
                  onChange={(e) =>
                    setForm({ ...form, claimedAmount: e.target.value })
                  }
                  placeholder="5000.00"
                  required
                />
              </div>
              <div className="form-group" style={{ gridColumn: '1 / -1' }}>
                <label>Description</label>
                <textarea
                  value={form.description}
                  onChange={(e) =>
                    setForm({ ...form, description: e.target.value })
                  }
                  placeholder="Describe the claim..."
                  required
                />
              </div>
            </div>
            <button type="submit" className="btn btn-primary">
              Submit Claim
            </button>
          </form>
        </div>
      </div>
    );
  }

  // DETAIL VIEW
  if (view === 'detail') {
    const status = claim?.currentStatus || claim?.status;
    const validTransitions = getValidTransitions();

    return (
      <div>
        <button className="back-link" onClick={() => { setView('list'); setClaim(null); setFraudResult(null); setMessage(null); }}>
          &larr; Back to Claims
        </button>

        <div className="page-header">
          <h2>Claim Detail</h2>
          <p>ID: {claimId}</p>
        </div>

        {message && (
          <div className={`message ${message.type}`}>{message.text}</div>
        )}

        {loading ? (
          <div className="loading">Loading claim...</div>
        ) : claim ? (
          <div className="claim-detail-grid">
            <div>
              <div className="card">
                <h3>Claim Information</h3>
                <div className="detail-row">
                  <span className="detail-label">Status</span>
                  <StatusBadge status={status} />
                </div>
                <div className="detail-row">
                  <span className="detail-label">Description</span>
                  <span className="detail-value">{claim.description}</span>
                </div>
                <div className="detail-row">
                  <span className="detail-label">Claimed Amount</span>
                  <span className="detail-value">
                    ${claim.claimedAmount?.toFixed(2)}
                  </span>
                </div>
                <div className="detail-row">
                  <span className="detail-label">Policy ID</span>
                  <span className="detail-value" style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                    {claim.policyId}
                  </span>
                </div>
                <div className="detail-row">
                  <span className="detail-label">Created</span>
                  <span className="detail-value">
                    {claim.createdAt
                      ? new Date(claim.createdAt).toLocaleString()
                      : 'N/A'}
                  </span>
                </div>
              </div>

              <div className="card">
                <h3>Transition Claim</h3>
                <div className="form-grid">
                  <div className="form-group">
                    <label>Changed By</label>
                    <input
                      type="text"
                      value={transitionForm.changedBy}
                      onChange={(e) =>
                        setTransitionForm({
                          ...transitionForm,
                          changedBy: e.target.value,
                        })
                      }
                      placeholder="Agent name"
                    />
                  </div>
                  <div className="form-group">
                    <label>Notes</label>
                    <input
                      type="text"
                      value={transitionForm.notes}
                      onChange={(e) =>
                        setTransitionForm({
                          ...transitionForm,
                          notes: e.target.value,
                        })
                      }
                      placeholder="Optional notes..."
                    />
                  </div>
                </div>
                <div className="transitions-bar">
                  {triggers
                    .filter((t) => validTransitions.includes(t.value))
                    .map((t) => (
                      <button
                        key={t.value}
                        className={`btn btn-sm ${t.btn}`}
                        onClick={() => handleTransition(t.value)}
                      >
                        {t.label}
                      </button>
                    ))}
                  {validTransitions.length === 0 && (
                    <span style={{ color: '#636e72', fontSize: '0.9rem' }}>
                      No transitions available for current status.
                    </span>
                  )}
                </div>
              </div>

              <div className="card">
                <h3>Fraud Check</h3>
                <button className="btn btn-warning" onClick={handleFraudCheck}>
                  Run Fraud Check
                </button>
                {fraudResult && (
                  <div style={{ marginTop: 16 }}>
                    <div
                      className={`fraud-score ${getFraudScoreClass(
                        fraudResult.riskScore
                      )}`}
                    >
                      Risk Score: {fraudResult.riskScore}
                    </div>
                    {fraudResult.riskFactors &&
                      fraudResult.riskFactors.length > 0 && (
                        <div className="risk-factors">
                          <strong style={{ fontSize: '0.85rem' }}>
                            Risk Factors:
                          </strong>
                          {fraudResult.riskFactors.map((f, i) => (
                            <div key={i} className="risk-factor">
                              {f}
                            </div>
                          ))}
                        </div>
                      )}
                  </div>
                )}
              </div>
            </div>

            <div>
              <div className="card">
                <h3>Status History</h3>
                {claim.statusHistory && claim.statusHistory.length > 0 ? (
                  <div className="timeline">
                    {claim.statusHistory.map((entry, i) => (
                      <div key={i} className="timeline-item">
                        <StatusBadge status={entry.status} />
                        <div className="timeline-date">
                          {entry.changedAt
                            ? new Date(entry.changedAt).toLocaleString()
                            : ''}
                          {entry.changedBy && ` by ${entry.changedBy}`}
                        </div>
                        {entry.notes && (
                          <div className="timeline-notes">{entry.notes}</div>
                        )}
                      </div>
                    ))}
                  </div>
                ) : (
                  <p style={{ color: '#636e72' }}>No status history available.</p>
                )}
              </div>
            </div>
          </div>
        ) : (
          <p>No claim data.</p>
        )}
      </div>
    );
  }

  return null;
}

export default Claims;
