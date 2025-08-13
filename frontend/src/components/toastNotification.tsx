import React, { useEffect, useState } from 'react';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastNotificationProps {
    message: string | null;
    type: ToastType;
    duration?: number; // in milliseconds, default 4 seconds
    onClose?: () => void;
}

const ToastNotification: React.FC<ToastNotificationProps> = ({
    message,
    type,
    duration = 4000,
    onClose
}) => {
    const [isVisible, setIsVisible] = useState(false);
    const [timeoutId, setTimeoutId] = useState<number | null>(null);

    useEffect(() => {
        // Clear any existing timeout
        console.log("Setting up toast notification for message:", message);
        if (timeoutId) {
            window.clearTimeout(timeoutId);
        }

        // Show the toast when message changes
        if (message) {
            setIsVisible(true);

            // Set timeout to hide the toast
            const newTimeoutId = window.setTimeout(() => {
                setIsVisible(false);
                onClose?.();
            }, duration);

            setTimeoutId(newTimeoutId);
        }

        // Cleanup function
        return () => {
            if (timeoutId) {
                window.clearTimeout(timeoutId);
            }
        };
    }, [message, type, duration]);

    if (!isVisible || !message) {
        return null;
    }

    const getToastStyles = () => {
        const baseStyles = "fixed top-4 right-4 px-6 py-3 rounded-lg shadow-lg z-50 max-w-sm transition-all duration-300 ease-in-out";

        switch (type) {
            case 'success':
                return `${baseStyles} bg-green-500 text-white`;
            case 'error':
                return `${baseStyles} bg-red-500 text-white`;
            case 'warning':
                return `${baseStyles} bg-yellow-500 text-black`;
            case 'info':
                return `${baseStyles} bg-blue-500 text-white`;
            default:
                return `${baseStyles} bg-gray-500 text-white`;
        }
    };

    const handleClose = () => {
        setIsVisible(false);
        if (timeoutId) {
            window.clearTimeout(timeoutId);
        }
        onClose?.();
    };

    return (
        <div className={getToastStyles()}>
            <div className="flex items-center justify-between">
                <span className="text-sm font-medium">{message}</span>
                <button
                    onClick={handleClose}
                    className="ml-4 text-lg font-bold opacity-70 hover:opacity-100 transition-opacity"
                    aria-label="Close notification"
                >
                    ×
                </button>
            </div>
        </div>
    );
};

export default ToastNotification;