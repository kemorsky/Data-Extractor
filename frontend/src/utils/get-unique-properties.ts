// Helper function to get unique values per property from an array 

export const getUniqueProperties = <T, K extends keyof T>(
    array: T[] | undefined,
    key: K
    ): NonNullable<T[K]>[] => {
    return [
        ...new Set(
        (array ?? [])
            .map(item => item[key])
            .filter((value): value is NonNullable<T[K]> => value != null)
        ),
    ];
};