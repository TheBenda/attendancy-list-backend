FROM hashicorp/vault:latest

# Run as root only long enough to prepare directories, then drop privileges
USER root

# Create directories for config, file storage, and audit logs
RUN mkdir -p /vault/config /vault/file /vault/logs \
    && chown -R vault:vault /vault/config /vault/file /vault/logs \
    && chmod 750 /vault/config /vault/file /vault/logs

# Copy Vault configuration
COPY vault.hcl /vault/config/vault.hcl

# Restrict config file permissions
RUN chown vault:vault /vault/config/vault.hcl \
    && chmod 640 /vault/config/vault.hcl

# Switch back to vault user
USER vault

# Vault UI + API
EXPOSE 8200

COPY vault.hcl /vault/config/vault.hcl

# Use config file
ENTRYPOINT ["vault"]
CMD ["server", "-config=/vault/config/vault.hcl"]