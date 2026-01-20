import { useState, useCallback } from "react";
import { AlertType } from "../components/CustomAlert";

interface AlertState {
  message: string;
  type: AlertType;
  isVisible: boolean;
}

export const useAlert = () => {
  const [alertState, setAlertState] = useState<AlertState>({
    message: "",
    type: "info",
    isVisible: false,
  });

  const showAlert = useCallback((message: string, type: AlertType = "info") => {
    setAlertState({
      message,
      type,
      isVisible: true,
    });
  }, []);

  const hideAlert = useCallback(() => {
    setAlertState((prev) => ({
      ...prev,
      isVisible: false,
    }));
  }, []);

  return {
    alertState,
    showAlert,
    hideAlert,
  };
};
