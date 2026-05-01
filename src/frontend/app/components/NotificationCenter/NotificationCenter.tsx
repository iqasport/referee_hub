import { faBell, faTrashCan } from "@fortawesome/free-regular-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useMemo, useState } from "react";

import { useNavigate } from "../../utils/navigationUtils";
import {
  NotificationItem,
  useDeleteNotificationMutation,
  useGetNotificationsQuery,
  useMarkAllNotificationsReadMutation,
  useMarkNotificationReadMutation,
} from "../../store/serviceApi";
import { useNotificationsRealtime } from "../../hooks/useNotificationsRealtime";

type Props = {
  currentUserId: string;
};

const formatDate = (value: string) => {
  const date = new Date(value);
  return date.toLocaleString();
};

const NotificationCenter = ({ currentUserId }: Props) => {
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);

  const {
    data,
    isLoading,
    refetch,
  } = useGetNotificationsQuery(undefined, { pollingInterval: 30000 });

  const [markRead] = useMarkNotificationReadMutation();
  const [markAllRead] = useMarkAllNotificationsReadMutation();
  const [deleteNotification] = useDeleteNotificationMutation();

  useNotificationsRealtime({
    onRefresh: () => {
      refetch();
    },
  });

  const notifications = data?.notifications ?? [];

  const unreadCount = useMemo(
    () => notifications.filter((item) => !item.isRead).length,
    [notifications],
  );

  const navigateFromNotification = async (item: NotificationItem) => {
    if (!item.isRead) {
      await markRead({ id: item.id });
    }

    if (item.relatedEntityType === "Tournament" && item.relatedEntityId) {
      navigate(`/tournaments/${item.relatedEntityId}`);
      return;
    }

    if (item.relatedEntityType === "Team" && item.relatedEntityId) {
      navigate(`/teams/${item.relatedEntityId}/manage`);
      return;
    }

    if (item.relatedEntityType === "Ngb" && item.relatedEntityId) {
      navigate(`/national_governing_bodies/${item.relatedEntityId}`);
      return;
    }

    if (item.relatedEntityType === "Test" && item.relatedEntityId) {
      navigate(`/referees/${currentUserId}/tests/${item.relatedEntityId}`);
      return;
    }

    navigate("/");
  };

  const handleMarkAllRead = async () => {
    await markAllRead();
    refetch();
  };

  const handleDelete = async (event: React.MouseEvent, id: string) => {
    event.stopPropagation();
    await deleteNotification({ id });
    refetch();
  };

  return (
    <div className="relative mr-4">
      <button
        type="button"
        onClick={() => setOpen(!open)}
        className="text-white hover:text-gray-200"
        aria-label="Open notifications"
      >
        <span className="relative inline-flex">
          <FontAwesomeIcon icon={faBell} className="text-lg" />
          {unreadCount > 0 && (
            <span className="absolute -top-1 -right-1 translate-x-1/3 -translate-y-1/3 bg-red-600 text-white text-xs font-bold rounded-full min-w-5 h-5 px-1 flex items-center justify-center">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </span>
      </button>

      {open && (
        <>
          <button
            type="button"
            className="fixed inset-0"
            style={{ zIndex: 2999 }}
            onClick={() => setOpen(false)}
            aria-label="Close notifications"
          />
          <div
            className="absolute right-0 mt-2 bg-white text-black rounded-lg shadow-2xl max-h-80 overflow-hidden border border-gray-200"
            style={{
              width: "min(24rem, calc(100vw - 1rem))",
              minWidth: "20rem",
              zIndex: 3000,
            }}
          >
            <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200 bg-gray-50">
              <h3 className="text-sm font-semibold text-gray-900">Notifications</h3>
              {notifications.length > 0 && (
                <button
                  type="button"
                  onClick={handleMarkAllRead}
                  className="text-xs font-medium text-blue-600 hover:text-blue-700 transition-colors whitespace-nowrap"
                >
                  Mark all read
                </button>
              )}
            </div>

            <div className="overflow-y-auto max-h-64">
              {isLoading && (
                <div className="px-4 py-6 text-center">
                  <p className="text-xs text-gray-500">Loading...</p>
                </div>
              )}

              {!isLoading && notifications.length === 0 && (
                <div className="px-4 py-8 text-center">
                  <p className="text-xs text-gray-500">You are all caught up.</p>
                </div>
              )}

              {!isLoading && notifications.map((item) => (
                <button
                  type="button"
                  key={item.id}
                  onClick={() => navigateFromNotification(item)}
                  className={`w-full text-left px-4 py-2 border-b border-gray-100 transition-colors text-xs ${
                    item.isRead
                      ? "bg-white hover:bg-gray-50"
                      : "bg-blue-50 hover:bg-blue-100"
                  }`}
                >
                  <div className="flex items-start justify-between gap-2">
                    <div className="flex-1 min-w-0">
                      <p className="font-semibold text-gray-900 line-clamp-1">{item.title}</p>
                      <p className="text-gray-600 line-clamp-2 leading-tight mt-0.5">{item.message}</p>
                      <p className="text-gray-400 mt-1">{formatDate(item.createdAt)}</p>
                    </div>
                    <button
                      type="button"
                      onClick={(event) => handleDelete(event, item.id)}
                      className="flex-shrink-0 text-gray-400 hover:text-red-600 transition-colors mt-0.5"
                      aria-label="Delete notification"
                    >
                      <FontAwesomeIcon icon={faTrashCan} className="text-xs" />
                    </button>
                  </div>
                </button>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default NotificationCenter;
