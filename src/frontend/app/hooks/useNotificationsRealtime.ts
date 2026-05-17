import { useEffect } from "react";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

type RealtimeHandlers = {
  onRefresh: () => void;
};

export const useNotificationsRealtime = ({ onRefresh }: RealtimeHandlers) => {
  useEffect(() => {
    let active = true;
    let connection: HubConnection | null = null;

    const start = async () => {
      connection = new HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build();

      const refresh = () => {
        if (active) {
          onRefresh();
        }
      };

      connection.on("NotificationCreated", refresh);
      connection.on("NotificationRead", refresh);
      connection.on("AllNotificationsRead", refresh);
      connection.on("NotificationDeleted", refresh);
      connection.on("UnreadCountChanged", refresh);

      try {
        await connection.start();
      } catch {
        // Polling remains active as fallback if real-time connection fails.
      }
    };

    start();

    return () => {
      active = false;
      if (connection) {
        connection.stop();
      }
    };
  }, [onRefresh]);
};
