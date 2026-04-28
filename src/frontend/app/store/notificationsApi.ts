import { serviceApi } from "./serviceApi";

export type NotificationItem = {
  id: string;
  type: string;
  title: string;
  message: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  secondaryEntityId?: string;
  secondaryEntityType?: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
};

export type NotificationsResponse = {
  notifications: NotificationItem[];
  unreadCount: number;
};

export type UnreadCountResponse = {
  unreadCount: number;
};

export const notificationsApi = serviceApi.injectEndpoints({
  endpoints: (build) => ({
    getNotifications: build.query<NotificationsResponse, void>({
      query: () => ({
        url: "/api/v2/notifications",
      }),
    }),
    getUnreadCount: build.query<UnreadCountResponse, void>({
      query: () => ({
        url: "/api/v2/notifications/unread-count",
      }),
    }),
    markNotificationRead: build.mutation<NotificationItem, string>({
      query: (id) => ({
        url: `/api/v2/notifications/${id}/read`,
        method: "PATCH",
      }),
    }),
    markAllNotificationsRead: build.mutation<{ markedAsReadCount: number }, void>({
      query: () => ({
        url: "/api/v2/notifications/read-all",
        method: "PATCH",
      }),
    }),
    deleteNotification: build.mutation<void, string>({
      query: (id) => ({
        url: `/api/v2/notifications/${id}`,
        method: "DELETE",
      }),
    }),
  }),
});

export const {
  useGetNotificationsQuery,
  useGetUnreadCountQuery,
  useMarkNotificationReadMutation,
  useMarkAllNotificationsReadMutation,
  useDeleteNotificationMutation,
} = notificationsApi;
