USE CuidaBem;

-- 1) Usuários
INSERT INTO Usuario (Nome, Email, Senha, TipoUsuario)
VALUES ('José Carlos', 'jose@example.com', 'senha123hash', 'idoso'),
       ('Maria Cuidadora', 'maria@example.com', 'senha123hash', 'cuidador');

-- 2) Farmácia
INSERT INTO Farmacia (Nome, Endereco, Telefone)
VALUES ('Farmacia Central', 'Rua A, 100', '11999998888');

-- 3) Medicamento (associado ao José)
INSERT INTO Medicamento (UsuarioID, NomeMedicamento, Dosagem)
VALUES (1, 'Losartana', '50mg'), (1, 'Metformina', '500mg');

-- 4) Lembretes
INSERT INTO Lembrete (MedicamentoID, Horario, Frequencia)
VALUES (1, '08:00:00', 'Diário'), (2, '20:00:00', 'Diário');

-- 5) Consulta
INSERT INTO Consulta (UsuarioID, Medico, DataHora)
VALUES (1, 'Dr. Silva', '2025-06-05 14:00:00');

-- 6) Pedido e item (José pede Metformina)
INSERT INTO Pedido (UsuarioID, FarmaciaID)
VALUES (1, 1);

INSERT INTO Pedido_Item (PedidoID, MedicamentoID, Quantidade)
VALUES (LAST_INSERT_ID(), 2, 2);