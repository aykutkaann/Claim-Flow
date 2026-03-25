import React from 'react';
import { NavLink } from 'react-router-dom';

function Sidebar() {
  const links = [
    { to: '/', label: 'Dashboard', icon: '📊' },
    { to: '/tenants', label: 'Tenants', icon: '🏢' },
    { to: '/customers', label: 'Customers', icon: '👥' },
    { to: '/policies', label: 'Policies', icon: '📋' },
    { to: '/claims', label: 'Claims', icon: '📁' },
    { to: '/reports', label: 'Reports', icon: '📈' },
  ];

  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <h1>ClaimFlow</h1>
        <p className="sidebar-subtitle">Insurance Management</p>
      </div>
      <nav className="sidebar-nav">
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            end={link.to === '/'}
            className={({ isActive }) =>
              `sidebar-link ${isActive ? 'active' : ''}`
            }
          >
            <span className="sidebar-icon">{link.icon}</span>
            <span>{link.label}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}

export default Sidebar;
