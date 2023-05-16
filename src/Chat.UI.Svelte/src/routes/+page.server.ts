import { query } from '../client';

const channelsQuery = `
    query Channels {
        channels {
            id
            name
            description
        }
    }
`;

export async function load() {
    return {
        channels: await query(channelsQuery)
    }
}