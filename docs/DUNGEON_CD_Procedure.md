Dungeon Backend - Continuous Delivery Procedure
Purpose
The goal of the Continuous Delivery (CD) workflow is to automatically build and publish the Docker image of the Dungeon backend.
The CD workflow is separated from the CI workflow:
- CI checks the source code with lint, tests and build jobs.
- CD creates the Docker image and publishes it to the Docker registry.
The workflow is stored in:
.github/workflows/cd.yaml
1. Create a dedicated branch
A feature branch was created from dev:
feature/dungeon-cd
All CD changes were made on this branch before opening a Pull Request to dev.
This follows the project branching strategy:
feature/* -> dev -> main
2. Create the CD workflow
The file below was added to the repository:
.github/workflows/cd.yaml
The Dungeon CD workflow follows the same structure as the Player microservice CD workflow so that backend microservices use a consistent delivery process.
3. Workflow triggers
The workflow can start in two different ways.
Manual execution on dev
The workflow_dispatch trigger allows the workflow to be started manually from GitHub Actions.
Manual publication is restricted to the dev branch.
workflow_dispatch:
A manual run on dev publishes development Docker tags such as:
dev-latest
dev-<digest>
Application release
The workflow also starts automatically when an application release tag is pushed:
push:
  tags: ["v*"]
Example:
v1.0.0
v1.1.0
v2.0.0
A release publishes:
vX.Y.Z
main-latest
Both tags are created from the same Docker build.
4. Checkout the source code
The workflow first retrieves the repository source code with actions/checkout.
- name: Checkout source
  uses: actions/checkout@...
This gives the GitHub Actions runner access to the Dungeon source code and its Dockerfile.
5. Compute Docker image tags
The workflow determines which Docker tags must be created depending on how it was started.
For a manual run on dev:
dev-latest
For a release tag such as v1.2.0:
v1.2.0
main-latest
The Git commit SHA is also recorded and added to the Docker image metadata.
6. Configure Docker Buildx
Docker Buildx is configured before building the image.
- name: Set up Docker Buildx
  uses: docker/setup-buildx-action@...
Buildx is used by the workflow to build the Dungeon image for:
linux/amd64
7. Authenticate to the Docker registry
Before publishing the image, the workflow authenticates to Docker Hub using GitHub repository secrets.
username: ${{ secrets.DOCKERHUB_USERNAME }}
password: ${{ secrets.DOCKERHUB_TOKEN }}
The credentials are stored as GitHub Secrets and are therefore not written directly in the workflow source code.
8. Build and publish the Dungeon image
The Docker image is built from the Dockerfile located at the root of the Dungeon repository.
context: .
platforms: linux/amd64
push: true
After the build completes successfully, the image is pushed to the configured Docker repository.
The published image also contains OCI metadata identifying:
- the GitHub repository;
- the Git commit used to build the image.
9. Create an immutable development tag
For development publications, the workflow also creates a tag based on the Docker image digest:
dev-<digest>
Example:
dev-a12bc34d...
This provides an immutable reference to the exact Docker image that was produced.
dev-latest can change after another publication, while the digest-based tag identifies one exact image.
10. Verify the published image
After publication, the workflow verifies that every expected image tag exists in the registry.
Build
  -> Push
  -> Verify
If an expected image cannot be inspected, the workflow fails instead of reporting a successful delivery.
11. GitHub Actions summary
At the end of the workflow, a delivery summary is written to the GitHub Actions run.
It contains:
- the source of the publication;
- the Docker image digest;
- all published tags.
Example:
Docker image published

Source: dev commit <sha>
Digest: sha256:...
Tags:
- <dungeon-image>:dev-latest
- <dungeon-image>:dev-<digest>
Result
The final delivery process is:
Dungeon source code
        |
        v
   GitHub Actions
        |
        v
   Docker Buildx
        |
        v
   Docker image
        |
        v
    Docker Hub
For development:
dev
 -> manual CD run
 -> build Docker image
 -> publish dev-latest
 -> publish dev-<digest>
For a release:
vX.Y.Z tag
 -> automatic CD run
 -> build Docker image
 -> publish vX.Y.Z
 -> publish main-latest
This provides a repeatable and traceable way to publish the Dungeon backend Docker image without building and pushing it manually from a developer workstation.