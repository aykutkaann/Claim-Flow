import React, { useState, useEffect } from 'react';
import api from '../api';

const productTypes = [
  { value: 0, label: 'Health' },
  { value: 1, label: 'Auto' },
  { value: 2, label: 'Home' },
  { value: 3, label: 'Travel' },
];

function Policies() {
  const [policies, setPolicies] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [form, setForm] = useState({
    policyNumber: '',
    customerId: '',
    productType: 0,
    startDate: '',
    endDate: '',
  });
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState(null);

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    setLoading(true);
    try {
      const [polRes, custRes] = await Promise.all([
        api.get('/api/policies'),
        api.get('/api/customers'),
      ]);
      setPolicies(polRes.data);
      setCustomers(custRes.data);
    } catch (err) {
      setMessage({ type: 'error', text: 'Failed to load data.' });
    }
    setLoading(false);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setMessage(null);
    try {
      await api.post('/api/policies', {
        ...form,
        productType: Number(form.productType),
      });
      setMessage({ type: 'success', text: 'Policy created successfully!' });
      setForm({
        policyNumber: '',
        customerId: '',
        productType: 0,
        startDate: '',
        endDate: '',
      });
      loadData();
    } catch (err) {
      const errorMsg =
        err.response?.data?.title || err.response?.data || 'Failed to create policy.';
      setMessage({ type: 'error', text: String(errorMsg) });
    }
  }

  function getProductLabel(val) {
    const pt = productTypes.find((p) => p.value === val);
    return pt ? pt.label : val;
  }

  return (
    <div>
      <div className="page-header">
        <h2>Policies</h2>
        <p>Manage insurance policies</p>
      </div>

      {message && (
        <div className={`message ${message.type}`}>{message.text}</div>
      )}

      <div className="card">
        <h3>Create Policy</h3>
        <form onSubmit={handleSubmit}>
          <div className="form-grid">
            <div className="form-group">
              <label>Policy Number</label>
              <input
                type="text"
                value={form.policyNumber}
                onChange={(e) =>
                  setForm({ ...form, policyNumber: e.target.value })
                }
                placeholder="POL-001"
                required
              />
            </div>
            <div className="form-group">
              <label>Customer</label>
              <select
                value={form.customerId}
                onChange={(e) =>
                  setForm({ ...form, customerId: e.target.value })
                }
                required
              >
                <option value="">Select a customer...</option>
                {customers.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.fullName} ({c.email})
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Product Type</label>
              <select
                value={form.productType}
                onChange={(e) =>
                  setForm({ ...form, productType: e.target.value })
                }
              >
                {productTypes.map((pt) => (
                  <option key={pt.value} value={pt.value}>
                    {pt.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Start Date</label>
              <input
                type="date"
                value={form.startDate}
                onChange={(e) =>
                  setForm({ ...form, startDate: e.target.value })
                }
                required
              />
            </div>
            <div className="form-group">
              <label>End Date</label>
              <input
                type="date"
                value={form.endDate}
                onChange={(e) => setForm({ ...form, endDate: e.target.value })}
                required
              />
            </div>
          </div>
          <button type="submit" className="btn btn-primary">
            Create Policy
          </button>
        </form>
      </div>

      <div className="card">
        <h3>All Policies ({policies.length})</h3>
        {loading ? (
          <div className="loading">Loading...</div>
        ) : (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Policy Number</th>
                  <th>Customer ID</th>
                  <th>Product Type</th>
                  <th>Start Date</th>
                  <th>End Date</th>
                </tr>
              </thead>
              <tbody>
                {policies.map((p) => (
                  <tr key={p.id}>
                    <td style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                      {p.id?.substring(0, 8)}...
                    </td>
                    <td>
                      <strong>{p.policyNumber}</strong>
                    </td>
                    <td style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                      {p.customerId?.substring(0, 8)}...
                    </td>
                    <td>{getProductLabel(p.productType)}</td>
                    <td>{new Date(p.startDate).toLocaleDateString()}</td>
                    <td>{new Date(p.endDate).toLocaleDateString()}</td>
                  </tr>
                ))}
                {policies.length === 0 && (
                  <tr>
                    <td colSpan="6" style={{ textAlign: 'center', color: '#636e72' }}>
                      No policies found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default Policies;
