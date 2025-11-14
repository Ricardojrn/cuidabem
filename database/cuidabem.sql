CREATE DATABASE CuidaBem;
USE CuidaBem;

-- ===============================
-- TABELA: USUARIO
-- ===============================
CREATE TABLE Usuario (
    UsuarioID INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    Senha VARCHAR(255) NOT NULL,
    TipoUsuario ENUM('idoso', 'cuidador', 'familia', 'admin') DEFAULT 'idoso'
);

-- ===============================
-- TABELA: FARMACIA
-- ===============================
CREATE TABLE Farmacia (
    FarmaciaID INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Endereco VARCHAR(150),
    Telefone VARCHAR(20)
);

-- ===============================
-- TABELA: MEDICAMENTO
-- ===============================
CREATE TABLE Medicamento (
    MedicamentoID INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioID INT NOT NULL,
    NomeMedicamento VARCHAR(100) NOT NULL,
    Dosagem VARCHAR(50),
    FOREIGN KEY (UsuarioID) REFERENCES Usuario(UsuarioID)
        ON DELETE CASCADE ON UPDATE CASCADE
);

-- ===============================
-- TABELA: LEMBRETE
-- ===============================
CREATE TABLE Lembrete (
    LembreteID INT AUTO_INCREMENT PRIMARY KEY,
    MedicamentoID INT NOT NULL,
    Horario TIME NOT NULL,
    Frequencia VARCHAR(50),
    FOREIGN KEY (MedicamentoID) REFERENCES Medicamento(MedicamentoID)
        ON DELETE CASCADE ON UPDATE CASCADE
);

-- ===============================
-- TABELA: CONSULTA
-- ===============================
CREATE TABLE Consulta (
    ConsultaID INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioID INT NOT NULL,
    Medico VARCHAR(100),
    DataHora DATETIME NOT NULL,
    FOREIGN KEY (UsuarioID) REFERENCES Usuario(UsuarioID)
        ON DELETE CASCADE ON UPDATE CASCADE
);

-- ===============================
-- TABELA: PEDIDO
-- ===============================
CREATE TABLE Pedido (
    PedidoID INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioID INT NOT NULL,
    FarmaciaID INT NOT NULL,
    DataPedido DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UsuarioID) REFERENCES Usuario(UsuarioID)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (FarmaciaID) REFERENCES Farmacia(FarmaciaID)
        ON DELETE CASCADE ON UPDATE CASCADE
);

-- ===============================
-- TABELA: PEDIDO_ITEM
-- ===============================
CREATE TABLE Pedido_Item (
    PedidoItemID INT AUTO_INCREMENT PRIMARY KEY,
    PedidoID INT NOT NULL,
    MedicamentoID INT NOT NULL,
    Quantidade INT NOT NULL,
    FOREIGN KEY (PedidoID) REFERENCES Pedido(PedidoID)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (MedicamentoID) REFERENCES Medicamento(MedicamentoID)
        ON DELETE CASCADE ON UPDATE CASCADE
);