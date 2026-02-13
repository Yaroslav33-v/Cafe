DROP PROCEDURE IF EXISTS do_payment_attempt;

DROP TRIGGER IF EXISTS trg_process_transfer ON operations;

DROP FUNCTION IF EXISTS process_transfer_operation() CASCADE;

DROP TABLE IF EXISTS operations CASCADE;
DROP TABLE IF EXISTS cards CASCADE;