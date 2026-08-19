export const getObjectCount = <T, K extends keyof T>(items: T[], key: K) => {
    return items.reduce((acc, item) => {
        const val = item[key];
        if (Array.isArray(val)) {
            // If the property is an array, count each element individually
            val.forEach((subItem) => {
                if (subItem !== undefined && subItem !== null) {
                    const strVal = String(subItem);
                    acc[strVal] = (acc[strVal] ?? 0) + 1;
                }
            });
        } else if (val !== undefined && val !== null) {
            // Handle single values
            const strVal = String(val);
            acc[strVal] = (acc[strVal] ?? 0) + 1;
        }

        return acc;
    }, {} as Record<string, number>);
};