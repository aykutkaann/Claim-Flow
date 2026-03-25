import React, { useState, useEffect } from 'react';
import api from '../api';
import StatusBadge from '../components/StatusBadge';

function Dashboard() {
  const [stats, setStats] = useState({
    tenants: 0,
    customers: 0,
    policies: 0,
  });
  const [health, setHealth] = useState({ status: 'checking', message: '' });
  const [recentClaims, setRecentClaims] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboard();
  }, []);

  async function loadDashboard() {
    setLoading(true);
    try {
      const [tenantsRes, customersRes, policiesRes, healthRes] =
        await Promise.allSettled([
          api.get('/api/tenants'),
          api.get('/api/customers'),
          api.get('/api/policies'),
          api.get('/health/ready'),
        ]);

      setStats({
        tenants:
          tenantsRes.status === 'fulfilled'
            ? tenantsRes.value.data.length
            : 0,
        customers:
          customersRes.status === 'fulfilled'
            ? customersRes.value.data.length
            : 0,
        policies:
          policiesRes.status === 'fulfilled'
            ? policiesRes.value.data.length
            : 0,
      });

      if (healthRes.status === 'fulfilled') {
        setHealth({ status: 'healthy', message: 'All systems operational' });
      } else {
        setHealth({ status: 'unhealthy', message: 'API is unreachable' });
      }

      // Try to gather recent claims from policies
      if (policiesRes.status === 'fulfilled') {
        const policies = policiesRes.value.data;
        const claimPromises = policies.slice(0, 10).map((p) =>
          api.get(`/api/claims/${p.id}`).catch(() => null)
        );
        const claimResults = await Promise.allSettled(claimPromises);
        const claims = claimResults
          .filter((r) => r.status === 'fulfilled' && r.value && r.value.data)
          .map((r) => r.value.data);
        setRecentClaims(claims.slice(0, 5));
      }
    } catch (err) {
      setHealth({ status: 'unhealthy', message: 'Failed to load dashboard' });
    }
    setLoading(false);
  }

  if (loading) {
    return <div className="loading">Loading dashboard...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <h2>Dashboard</h2>
        <p>Overview of your insurance management system</p>
      </div>

      <div className="stats-grid">
        <div className={`stat-card ${health.status === 'healthy' ? 'healthy' : 'unhealthy'}`}>
          <span className="stat-label">System Health</span>
          <span className="stat-value">
            {health.status === 'healthy' ? 'OK' : 'DOWN'}
          </span>
          <span style={{ fontSize: '0.8rem', color: '#636e72', marginTop: 4 }}>
            {health.message}
          </span>
        </div>
        <div className="stat-card">
          <span className="stat-label">Tenants</span>
          <span className="stat-value">{stats.tenants}</span>
        </div>
        <div className="stat-card">
          <span className="stat-label">Customers</span>
          <span className="stat-value">{stats.customers}</span>
        </div>
        <div className="stat-card">
          <span className="stat-label">Policies</span>
          <span className="stat-value">{stats.policies}</span>
        </div>
      </div>

      <div className="card">
        <h3>Recent Claims</h3>
        {recentClaims.length === 0 ? (
          <p style={{ color: '#636e72' }}>No claims found.</p>
        ) : (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Description</th>
                  <th>Amount</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {recentClaims.map((claim) => (
                  <tr key={claim.id}>
                    <td>{claim.id}</td>
                    <td>{claim.description}</td>
                    <td>${claim.claimedAmount?.toFixed(2)}</td>
                    <td>
                      <StatusBadge status={claim.currentStatus || claim.status} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default Dashboard;
