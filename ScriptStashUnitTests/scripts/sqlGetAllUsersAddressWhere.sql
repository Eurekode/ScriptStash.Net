SELECT [CSV_FIELDS]
FROM USERS INNER JOIN ADDRESS ON USERS.USER_ID = ADDRESS.USER_ID
WHERE [USERS_CONDITION]