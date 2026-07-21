export const getObjectCount = <T, K extends keyof T>(items: T[], key: K) => {
    return items.reduce((acc, item) => {
        const value = String(item[key]);
        acc[value] = (acc[value ?? 0] ?? 0) + 1;
        return acc
    }, {} as Record<string, number>);
};