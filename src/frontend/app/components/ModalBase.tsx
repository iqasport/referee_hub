import React from "react";
import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";

interface ModalBaseProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  subtitle?: string;
  maxWidth?: "sm" | "md" | "lg" | "xl" | "2xl";
  children: React.ReactNode;
}

const maxWidthClasses = {
  sm: "max-w-sm",
  md: "max-w-md",
  lg: "max-w-lg",
  xl: "max-w-xl",
  "2xl": "max-w-2xl",
};

const ModalBase: React.FC<ModalBaseProps> = ({
  isOpen,
  onClose,
  title,
  subtitle,
  maxWidth = "2xl",
  children,
}) => {
  return (
    <Dialog open={isOpen} as="div" className="relative z-50" onClose={onClose}>
      <div
        className="fixed inset-0"
        style={{ backgroundColor: "rgba(0, 0, 0, 0.3)" }}
        aria-hidden="true"
      />
      <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
        <DialogPanel
          className={`relative w-full ${maxWidthClasses[maxWidth]} rounded-lg bg-white p-6 shadow-xl my-8 overflow-y-auto`}
          style={{ maxHeight: "90vh" }}
        >
          <DialogTitle as="h3" className="text-xl font-semibold text-gray-900 mb-1">
            {title}
          </DialogTitle>
          {subtitle && <p className="text-sm text-gray-600 mb-4">{subtitle}</p>}
          {children}
        </DialogPanel>
      </div>
    </Dialog>
  );
};

export default ModalBase;
