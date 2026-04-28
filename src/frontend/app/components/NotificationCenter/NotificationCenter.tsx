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
} from "../../store/notificationsApi";
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
      await markRead(item.id);
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
    await deleteNotification(id);
    refetch();
  };

  return (
    <div className="relative mr-4">
      <button
        type="button"
        onClick={() => setOpen(!open)}
        className="relative text-white hover:text-gray-200"
        aria-label="Open notifications"
      >
        <FontAwesomeIcon icon={faBell} className="text-lg" />
        {unreadCount > 0 && (
          <span className="absolute -top-2 -right-2 bg-red-600 text-white text-xs font-bold rounded-full min-w-5 h-5 px-1 flex items-center justify-center">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <>
          <button
            type="button"
            className="fixed inset-0"
            onClick={() => setOpen(false)}
            aria-label="Close notifications"
          />
          <div className="absolute right-0 mt-3 w-96 bg-white text-black rounded shadow-xl z-10 max-h-[70vh] overflow-hidden">
            <div className="flex items-center justify-between px-4 py-3 border-b">
              <h3 className="font-semibold">Notifications</h3>
              <button
                type="button"
                onClick={handleMarkAllRead}
                className="text-sm text-blue-700 hover:text-blue-900"
              >
                Mark all read
              </button>
            </div>

            <div className="overflow-y-auto max-h-[60vh]">
              {isLoading && <p className="px-4 py-3 text-sm">Loading notifications...</p>}

              {!isLoading && notifications.length === 0 && (
                <p className="px-4 py-8 text-sm text-gray-600">You are all caught up.</p>
              )}

              {!isLoading && notifications.map((item) => (
                <button
                  type="button"
                  key={item.id}
                  onClick={() => navigateFromNotification(item)}
                  className={`w-full text-left px-4 py-3 border-b hover:bg-gray-50 ${item.isRead ? "opacity-70" : "bg-blue-50"}`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-semibold text-sm">{item.title}</p>
                      <p className="text-sm text-gray-700">{item.message}</p>
                      <p className="text-xs text-gray-500 mt-1">{formatDate(item.createdAt)}</p>
                    </div>
                    <button
                      type="button"
                      onClick={(event) => handleDelete(event, item.id)}
                      className="text-gray-500 hover:text-red-600"
                      aria-label="Delete notification"
                    >
                      <FontAwesomeIcon icon={faTrashCan} />
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
