export interface PollingOptions {
    maxRetries?: number;
    baseRetryDelay?: number;
    signal?: AbortSignal;
}

/**
 * Create a polling generator function that periodically fetches data from an API
 * and yields the response data. It uses a retry mechanism if fetching fails.
 *
 * @param request - The Request object to fetch.
 * @param interval - The interval between each poll in milliseconds.
 * @param options - Configuration options for the polling operation. Optional.
 * 
 * @example
 * ```typescript
 * const controller = new AbortController();
 * const request = new Request("https://api.example.com/data", { signal: controller.signal });
 * const generator = pollFetch<MyDataType>(request, 5000);
 * 
 * (async () => {
 *     try {
 *         for await (let data of generator) {
 *             console.log(data);
 *         }
 *     } catch (error) {
 *         console.error(error);
 *     }
 * })();
 * 
 * // To cancel the operation at any point, call:
 * // controller.abort();
 * ```
 *
 * @returns An asynchronous generator that yields data of type T from the API.
 */
export async function* pollFetch<T = any>(url: string, init: RequestInit, interval: number, options: PollingOptions = { maxRetries: 3, baseRetryDelay: 1000 }): AsyncIterableIterator<T> {
    while (true) {
        if (options.signal && options.signal.aborted) {
            return;
        }

        let retries = 0;
        while (retries <= options.maxRetries!) {
            try {
                let response = await fetch(url, init);
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                } else {
                    let data: T = await response.json();
                    yield data;
                    break;
                }
            } catch (error) {
                if (options.signal && options.signal.aborted) {
                    return;
                }

                if (retries === options.maxRetries!) {
                    throw error;
                }
                let jitter = Math.random() * options.baseRetryDelay!;
                await new Promise(resolve => setTimeout(resolve, options.baseRetryDelay! + jitter));
                retries++;
            }
        }
        await new Promise(resolve => setTimeout(resolve, interval));
    }
}
