-- El efecto neto aplicado al balance se guarda al publicar; los reversos usan este delta.
-- Sin él, cancelar un conteo restaba la cantidad contada en lugar del ajuste real aplicado.
ALTER TABLE "InventoryMovement" ADD COLUMN "appliedDelta" DECIMAL(14,3);
