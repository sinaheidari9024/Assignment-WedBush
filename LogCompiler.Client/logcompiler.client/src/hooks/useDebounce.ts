import { useEffect, useRef } from 'react';

export const useDebounce = <T = unknown>(
    callback: () => void,
    delay: number,
    dependencies: T[] = []
) => {
    const timeoutRef = useRef<number | undefined>(undefined);

    useEffect(() => {
        timeoutRef.current = window.setTimeout(callback, delay);

        return () => {
            if (timeoutRef.current) {
                window.clearTimeout(timeoutRef.current);
            }
        };
    }, [callback, delay, ...dependencies]);
};