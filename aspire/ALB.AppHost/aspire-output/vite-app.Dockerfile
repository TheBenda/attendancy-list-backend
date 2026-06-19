FROM node:22-slim AS build
WORKDIR /app
RUN corepack enable pnpm
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN --mount=type=cache,target=/pnpm/store pnpm install --frozen-lockfile
COPY . .
RUN pnpm run build

FROM mcr.microsoft.com/dotnet/nightly/yarp:2.3-preview AS runtime
WORKDIR /app
COPY --from=build /app/dist /app/wwwroot
ENTRYPOINT ["dotnet","/app/yarp.dll"]
