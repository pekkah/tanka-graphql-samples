import wsFactory from "./ws-client";
import tankaFactory from "./tanka-client";

export default function clientFactory(protocol) {
    console.log(`Using: ${protocol}`);
    if (protocol === "ws")
        return wsFactory();

    return tankaFactory();
}