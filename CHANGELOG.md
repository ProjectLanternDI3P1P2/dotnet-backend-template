# Changelog

## [1.2.0](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/compare/v1.1.0...v1.2.0) (2026-09-27)


### Added

* **database:** migrate and seed development data ([64a6992](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/64a6992c07c1f7510c39810263a12a688d304c7b))
* **database:** migrate and seed development data ([7c77c8b](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/7c77c8bec35cb6dba37568f0c3ba38297bcc6314))
* **grpc:** propagate correlation IDs ([ff71dde](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/ff71ddebda9e7b6db5b0e72a3142e56a216987cd))
* **grpc:** propagate correlation IDs ([d31b1af](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/d31b1af279eff3836148f0ed3f54882094b045b3))
* **openapi:** persist user ID in Scalar ([9ec7b32](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/9ec7b3256facc8f14894a1c4f32cef252c3a78f8))
* **openapi:** persist user ID in Scalar ([f837244](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/f837244c80f77f5e8b929a72fe761a834094fc0c))
* **template:** complete database and integration examples ([a3ed989](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/a3ed989b90febd9633a06bcddb733393524ba06d))


### Fixed

* **ci:** provision PostgreSQL for Sonar tests ([125c52a](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/125c52ae5fd0a7e76ff4ada9089b6a19d3ab1b50))
* **database:** retain migration workflow ([791644c](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/791644c7e7669605c7c753b87d1816502d6568e5))
* **docker:** include dockerignore in application project ([fdedfc4](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/fdedfc42df92463c9fed47d76c222c3f11911dde))
* **docker:** include dockerignore in application project ([6014816](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/60148164bcc39252c68ae46fc06e1f96d68d9e84))

## [1.1.0](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/compare/v1.0.0...v1.1.0) (2026-09-23)


### Added

* add ef core migration tooling ([b3abca4](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/b3abca46187ee86de0eda40e2da4ef26ef68e275))
* add ef core migration tooling ([e62b3f1](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/e62b3f12e77dce1622db927db6ad55cceaf05166))
* enforce command transaction boundary ([4096387](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/40963875235dde96cbfbe0d22352baf86f1d5255))
* publish player created events through RabbitMQ ([40727b6](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/40727b6125a422e1cbbeefaab2d17520c4ce2650))
* publish player created events through RabbitMQ ([cccf22e](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/cccf22eebfad251aee42150c4ba83fea8879c20c))

## [1.0.0](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/compare/v0.1.1...v1.0.0) (2026-09-22)


### ⚠ BREAKING CHANGES

* **contracts:** release protobuf contracts independently

### Added

* **contracts:** publish protobuf package ([5df7049](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/5df7049dcb876f7f821bc58759f0fef43d7e6ec6))
* **contracts:** release protobuf contracts independently ([d7cefcb](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/d7cefcb8b517075887b9f33c90c4262bac64a212))
* **grpc:** add infrastructure player client ([00a2531](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/00a2531a16d93773e7bed45485eefc608314030a))
* **grpc:** expose player service internally ([b374c80](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/b374c80622924deaa333dabc33f350175f8b1e88))
* Mise en place d'un exemple de service GRPC client et serveur ([550c759](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/550c759e0f5de9d7d8cebb07b2f489016918cb07))

## [0.1.1](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/compare/v0.1.0...v0.1.1) (2026-09-05)


### Fixed

* run the container as the unprivileged app user ([#8](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/issues/8)) ([894be3f](https://github.com/ProjectLanternDI3P1P2/dotnet-backend-template/commit/894be3fe28e64218726b2dbc2e2bccb7efb9846b))
