import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faXmark } from "@fortawesome/free-solid-svg-icons";

export interface RosterMember {
  userId: string;
  userName: string;
  number?: string;
  gender?: string;
}

interface PlayersTableProps {
  members: RosterMember[];
  onAddClick: () => void;
  onRemove: (userId: string) => void;
  onUpdateMember?: (userId: string, field: "number" | "gender", value: string) => void;
  disabled?: boolean;
}

const PlayersTable: React.FC<PlayersTableProps> = ({
  members,
  onAddClick,
  onRemove,
  onUpdateMember,
  disabled = false,
}) => {
  return (
    <div style={{ 
      backgroundColor: '#eff6ff', 
      border: '1px solid #bfdbfe', 
      borderRadius: '0.5rem', 
      overflow: 'hidden',
      display: 'flex',
      flexDirection: 'column',
      height: '100%',
      maxHeight: '600px',
    }}>
      {/* Header */}
      <div style={{ 
        backgroundColor: '#dbeafe', 
        color: '#1e3a8a', 
        padding: '0.75rem 1rem', 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        flexShrink: 0,
      }}>
        <h3 style={{ fontWeight: '600', fontSize: '0.875rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
          PLAYERS ({members.length})
        </h3>
        <button
          type="button"
          onClick={onAddClick}
          disabled={disabled}
          style={{
            borderRadius: '9999px',
            transition: 'colors 0.2s',
            backgroundColor: disabled ? '#d1d5db' : '#fff',
            color: disabled ? '#6b7280' : '#16a34a',
            cursor: disabled ? 'not-allowed' : 'pointer',
            padding: '6px',
            width: '28px',
            height: '28px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            border: 'none',
            boxShadow: disabled ? 'none' : '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
          }}
          onMouseEnter={(e) => {
            if (!disabled) {
              e.currentTarget.style.backgroundColor = '#f3f4f6';
            }
          }}
          onMouseLeave={(e) => {
            if (!disabled) {
              e.currentTarget.style.backgroundColor = '#fff';
            }
          }}
          title="Add player"
        >
          <FontAwesomeIcon icon={faPlus} style={{ fontSize: '14px' }} />
        </button>
      </div>

      {/* Table Container with Scrollbar */}
      <div style={{ 
        padding: '0.75rem',
        overflowY: 'auto',
        overflowX: 'hidden',
        flex: 1,
        minHeight: 0,
      }}>
        <table style={{ width: '100%', borderCollapse: 'separate', borderSpacing: '0' }}>
          <thead>
            <tr style={{ backgroundColor: '#fff', borderBottom: '2px solid #bfdbfe' }}>
              <th style={{ 
                textAlign: 'left', 
                padding: '0.5rem', 
                fontSize: '0.75rem', 
                fontWeight: '600', 
                color: '#4b5563',
                textTransform: 'uppercase',
                letterSpacing: '0.05em',
              }}>
              Name
              </th>
              <th style={{ 
                textAlign: 'left', 
                padding: '0.5rem', 
                fontSize: '0.75rem', 
                fontWeight: '600', 
                color: '#4b5563',
                width: '80px',
                textTransform: 'uppercase',
                letterSpacing: '0.05em',
              }}>
                Number
              </th>
              <th style={{ 
                textAlign: 'left', 
                padding: '0.5rem', 
                fontSize: '0.75rem', 
                fontWeight: '600', 
                color: '#4b5563',
                width: '100px',
                textTransform: 'uppercase',
                letterSpacing: '0.05em',
              }}>
                Gender
              </th>
              <th style={{ 
                width: '40px',
                padding: '0.5rem',
              }}></th>
            </tr>
          </thead>
          <tbody>
            {members.length === 0 ? (
              <tr>
                <td colSpan={4} style={{ 
                  textAlign: 'center', 
                  color: '#6b7280', 
                  fontSize: '0.875rem', 
                  paddingTop: '2rem', 
                  paddingBottom: '2rem',
                  backgroundColor: '#fff',
                }}>
                  No players added yet.
                  <br />
                  <span style={{ fontSize: '0.75rem' }}>Click + to add players</span>
                </td>
              </tr>
            ) : (
              members.map((member, index) => (
                <tr 
                  key={member.userId}
                  style={{ 
                    backgroundColor: '#fff',
                    borderBottom: index < members.length - 1 ? '1px solid #e5e7eb' : 'none',
                  }}
                >
                  <td style={{ 
                    padding: '0.5rem', 
                    fontSize: '0.875rem',
                    fontWeight: '500',
                    color: '#111827',
                  }}>
                    {member.userName}
                  </td>
                  <td style={{ padding: '0.5rem' }}>
                    <input
                      type="text"
                      value={member.number || ""}
                      onChange={(e) => onUpdateMember?.(member.userId, "number", e.target.value)}
                      disabled={disabled}
                      placeholder="#"
                      maxLength={3}
                      style={{
                        width: '100%',
                        padding: '0.375rem 0.5rem',
                        fontSize: '0.875rem',
                        border: '1px solid #d1d5db',
                        borderRadius: '0.25rem',
                        backgroundColor: disabled ? '#f3f4f6' : '#fff',
                        color: '#111827',
                      }}
                    />
                  </td>
                  <td style={{ padding: '0.5rem' }}>
                    <input
                      type="text"
                      value={member.gender || ""}
                      onChange={(e) => onUpdateMember?.(member.userId, "gender", e.target.value)}
                      disabled={disabled}
                      placeholder="M/F"
                      maxLength={10}
                      style={{
                        width: '100%',
                        padding: '0.375rem 0.5rem',
                        fontSize: '0.875rem',
                        border: '1px solid #d1d5db',
                        borderRadius: '0.25rem',
                        backgroundColor: disabled ? '#f3f4f6' : '#fff',
                        color: '#111827',
                      }}
                    />
                  </td>
                  <td style={{ padding: '0.5rem', textAlign: 'center' }}>
                    <button
                      type="button"
                      onClick={() => onRemove(member.userId)}
                      disabled={disabled}
                      style={{
                        borderRadius: '0.25rem',
                        transition: 'colors 0.2s',
                        color: disabled ? '#9ca3af' : '#9ca3af',
                        cursor: disabled ? 'not-allowed' : 'pointer',
                        padding: '4px',
                        border: 'none',
                        backgroundColor: 'transparent',
                      }}
                      onMouseEnter={(e) => {
                        if (!disabled) {
                          e.currentTarget.style.color = '#ef4444';
                          e.currentTarget.style.backgroundColor = '#fef2f2';
                        }
                      }}
                      onMouseLeave={(e) => {
                        if (!disabled) {
                          e.currentTarget.style.color = '#9ca3af';
                          e.currentTarget.style.backgroundColor = 'transparent';
                        }
                      }}
                      title={`Remove ${member.userName}`}
                    >
                      <FontAwesomeIcon icon={faXmark} style={{ fontSize: '14px' }} />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Row Count Summary */}
      <div style={{
        backgroundColor: '#f9fafb',
        borderTop: '1px solid #bfdbfe',
        padding: '0.5rem 1rem',
        fontSize: '0.75rem',
        color: '#6b7280',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexShrink: 0,
      }}>
        <span>Total: {members.length} player{members.length !== 1 ? 's' : ''}</span>
      </div>
    </div>
  );
};

export default PlayersTable;
