import { useEffect, useRef } from 'react';
export const useDebounce = <T = unknown>(
    callback: () => void,
    delay: number,
    dependencies: T[] = []
) => {
    const timeoutRef = useRef<number | undefined>(undefined);

    useEffect(() => {
        if (timeoutRef.current) {
            clearTimeout(timeoutRef.current);
        }

        timeoutRef.current = setTimeout(() => {
            callback();
        }, delay);

        return () => {
            if (timeoutRef.current) {
                clearTimeout(timeoutRef.current);
            }
        };
    }, dependencies);
};