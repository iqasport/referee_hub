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

After setting up the environment I had to go into EC2 (VM) mapped to EB and edit the security group to allow inbound traffic to HTTPS port 443.

We're also using RDS (managed postgres) linked to EB. It auto injects DB credentials into the env (see Dockerfile).

## Data Protection key encryption

The application persists its ASP.NET Core Data Protection key ring in the RDS database. Production must also provide an independent certificate so a database-only disclosure does not disclose the key-encryption private key. This does not protect against compromise of the running application or its EC2 instance role, which can access both the database and certificate.

Provision a dedicated RSA certificate for Data Protection. Do not reuse the Certbot/Kestrel TLS certificate. Store the base64-encoded password-protected PFX and its password as separate AWS Secrets Manager secrets. In the Elastic Beanstalk environment's runtime environment variables, configure both values with **Source: Secrets Manager** (not Plain text):

- `DataProtection__CertificatePfxBase64`: base64-encoded password-protected PFX containing the certificate and private key.
- `DataProtection__CertificatePassword`: the PFX password.

The production Docker Compose file forwards these values to the application. Configure the EC2 instance profile with `secretsmanager:GetSecretValue` for those two secret ARNs; if the secrets use a customer-managed KMS key, also grant `kms:Decrypt` and update that key's policy. The application fails startup when either value is missing or invalid. Do not put the certificate, PFX password, or their values in source control, plain-text environment properties, the container image, or deployment artifacts. Using Secrets Manager references keeps the stored Elastic Beanstalk option values short and avoids its 4 KB combined plain environment-property limit.

Before the first deployment with this setting, take a database backup and schedule a maintenance window. The first new application instance encrypts existing Data Protection key XML in place after database migrations; key IDs and protected application data remain unchanged. Do not run old and new application versions concurrently during this conversion, because an old instance could write another unencrypted key. Verify startup and user-data reads before returning traffic.

Back up the PFX independently of RDS and retain it for as long as any protected data or encrypted key row depends on it. Loss of the private key makes those values unreadable. The current configuration uses one certificate and does not support in-place certificate rotation; keep it stable. Before rotating it, add old-certificate decryption support and test a staged key-ring rewrap, rather than simply overwriting or deleting the existing certificate.
