import OAuth, { type Configuration } from "solid-start-oauth";

const configuration: Configuration = env => ({
  github: {
    id: process.env.GOOGLE_ID as string,
    secret: process.env.GOOGLE_SECRET as string,
    state: process.env.STATE, //optional XSRF protection
  },
  async handler(user, redirect) {
    //use solid-start sessions to store cookie
    const dbUser = await database.getUser("email", user.email);
    if (dbUser) return await signIn(dbUser, redirect);
    return await signUp(user);
  },
});

export const GET = OAuth(configuration);