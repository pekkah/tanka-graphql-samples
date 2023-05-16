export function query(gql: string, variables?: Map<string, any>) {
    return fetch('https://localhost:8000/graphql', {
            method: 'POST',
            body: JSON.stringify({
                query: gql,
                variables: variables
            })
        }).then(res => res.json())
}