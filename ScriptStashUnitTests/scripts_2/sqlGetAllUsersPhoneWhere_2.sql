SELECT *
FROM USERS INNER JOIN PHONES ON USERS.USER_ID = PHONES.USER_ID
WHERE [USERS_CONDITION]