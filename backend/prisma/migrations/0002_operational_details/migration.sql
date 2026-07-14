ALTER TABLE "Activity" ADD COLUMN "pendingItems" TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[];
ALTER TABLE "DailyReport" ADD COLUMN "materials" JSONB;
ALTER TABLE "DailyReport" ADD COLUMN "machinery" JSONB;
ALTER TABLE "DailyReport" ADD COLUMN "fuelLiters" DECIMAL(14,3);
