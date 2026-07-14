-- La SPA nunca accede a estas tablas mediante PostgREST. La API Nest es el único punto de acceso.
DO $$
DECLARE app_table TEXT;
BEGIN
  FOREACH app_table IN ARRAY ARRAY[
    'User','RefreshToken','AuditLog','PermissionGrant','Material','Warehouse','Project','Platform','Activity','MaterialEstimate','InventoryBalance','InventoryMovement','Machine','MachineAssignment','FuelLog','MaintenancePlan','MaintenanceRecord','Repair','DailyReport','Alert'
  ] LOOP
    EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', app_table);
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'anon') THEN EXECUTE format('REVOKE ALL ON TABLE %I FROM anon', app_table); END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'authenticated') THEN EXECUTE format('REVOKE ALL ON TABLE %I FROM authenticated', app_table); END IF;
  END LOOP;
END $$;
