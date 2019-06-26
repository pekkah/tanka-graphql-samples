FROM node:8-alpine as base
WORKDIR /app
EXPOSE 3000

FROM base as build
WORKDIR /
COPY package.json ./
RUN npm install
COPY . .
RUN npm run build

FROM base as run
RUN npm install express \
    && npm install dotenv

COPY --from=build ./build ./
ENTRYPOINT [ "node", "server.js"]

