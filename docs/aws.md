# Amazon Web Services

## CLI
https://aws.amazon.com/cli/

## Identity (IAM)
AWS access for IQA is managed with Google workspaces.
See how it was setup: https://aws.amazon.com/blogs/security/how-to-use-g-suite-as-external-identity-provider-aws-sso/

GitHub actions integration was created using OIDC provider as per
https://github.com/aws-actions/configure-aws-credentials and
https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_create_for-idp_oidc.html#idp_oidc_Create_GitHub

```yaml
- name: Configure AWS Credentials
    uses: aws-actions/configure-aws-credentials@v2
    with:
        role-to-assume: arn:aws:iam::558550744343:role/ManagementHubGitHubActionsRole
        aws-region: us-east-1
```

See the permissions required for uploading a docker image: https://github.com/aws-actions/amazon-ecr-login#ecr-public

## Elastic container registry
After creating the container repository I wanted to setup automatic publish from GitHub actions.
So I followed https://docs.github.com/en/actions/deployment/deploying-to-your-cloud-provider/deploying-to-amazon-elastic-container-service

When working with ECR locally via AWS CLI make sure that your docker client is up to date.

## Elastic Beanstalk
We're using Beanstalk to deploy the containerized application to a VM.
https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/create_deploy_docker.html

We're using the Docker platform branch (not the ECS one).

## CloudFront (CDN)
We have a CloudFront on top of our service for hosting static files.
