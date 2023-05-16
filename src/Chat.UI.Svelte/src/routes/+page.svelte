<script>
    export let data;
</script>

<h1>GraphQL</h1>
<ul>
{#each data.channels as channel }
    <li>{channel.name}</li>
{/each}
</ul>
