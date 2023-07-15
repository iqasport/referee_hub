# This is a custom image on top of the official AWS OTEL distro
# For some reason I wasn't able to inject the config into it without putting it directly in the image
# Run 
#   docker build -t public.ecr.aws/g7w7d4k3/iqasport/aws-otel-collector:latest .
# in this folder to build the image, then push it
FROM public.ecr.aws/aws-observability/aws-otel-collector:latest

HEALTHCHECK --interval=15s --timeout=30s --start-period=5s --retries=3 CMD [ "/healthcheck" ]
COPY otel-config.yml /cfg/otel-config.yml
CMD ["--config=/cfg/otel-config.yml"]
