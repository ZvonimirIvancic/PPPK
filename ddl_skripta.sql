
CREATE SEQUENCE IF NOT EXISTS pacijenti_id_seq;
CREATE SEQUENCE IF NOT EXISTS med_dokumentacija_id_seq;
CREATE SEQUENCE IF NOT EXISTS pregledi_id_seq;
CREATE SEQUENCE IF NOT EXISTS slike_id_seq;
CREATE SEQUENCE IF NOT EXISTS recepti_id_seq;

-- Kreiranje tabele PACIJENTI
CREATE TABLE IF NOT EXISTS pacijenti (
    id SERIAL PRIMARY KEY,
    oib VARCHAR(11) NOT NULL UNIQUE,
    ime VARCHAR(100) NOT NULL,
    prezime VARCHAR(100) NOT NULL,
    datum_rodenja DATE NOT NULL,
    spol CHAR(1) CHECK (spol IN ('M', 'Ž')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Kreiranje tabele MEDICINSKA_DOKUMENTACIJA
CREATE TABLE IF NOT EXISTS medicinska_dokumentacija (
    id SERIAL PRIMARY KEY,
    pacijent_id INTEGER NOT NULL,
    naziv_bolesti VARCHAR(200) NOT NULL,
    datum_pocetka DATE NOT NULL,
    datum_zavrsetka DATE,
    opis TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pacijent_id) REFERENCES pacijenti(id) ON DELETE CASCADE
);

-- Kreiranje tabele PREGLEDI
CREATE TABLE IF NOT EXISTS pregledi (
    id SERIAL PRIMARY KEY,
    pacijent_id INTEGER NOT NULL,
    tip_pregleda VARCHAR(10) NOT NULL CHECK (
        tip_pregleda IN ('GP', 'KRV', 'X-RAY', 'CT', 'MR', 'ULTRA', 'EKG', 'ECHO', 'EYE', 'DERM', 'DENTA', 'MAMMO', 'NEURO')
    ),
    datum_pregleda DATE NOT NULL,
    vrijeme_pregleda TIME NOT NULL,
    opis TEXT,
    nalaz TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pacijent_id) REFERENCES pacijenti(id) ON DELETE CASCADE
);

-- Kreiranje tabele SLIKE
CREATE TABLE IF NOT EXISTS slike (
    id SERIAL PRIMARY KEY,
    pregled_id INTEGER NOT NULL,
    naziv_datoteke VARCHAR(255) NOT NULL,
    putanja VARCHAR(500) NOT NULL,
    tip_datoteke VARCHAR(50),
    velicina_datoteke BIGINT,
    datum_upload TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    opis TEXT,
    FOREIGN KEY (pregled_id) REFERENCES pregledi(id) ON DELETE CASCADE
);

-- Kreiranje tabele RECEPTI
CREATE TABLE IF NOT EXISTS recepti (
    id SERIAL PRIMARY KEY,
    pacijent_id INTEGER NOT NULL,
    pregled_id INTEGER,
    naziv_lijeka VARCHAR(200) NOT NULL,
    doza VARCHAR(100) NOT NULL,
    upute TEXT,
    datum_izdavanja DATE NOT NULL,
    datum_vazenja DATE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pacijent_id) REFERENCES pacijenti(id) ON DELETE CASCADE,
    FOREIGN KEY (pregled_id) REFERENCES pregledi(id) ON DELETE SET NULL
);

-- Kreiranje indeksa za bolju performance
CREATE INDEX idx_pacijenti_oib ON pacijenti(oib);
CREATE INDEX idx_pacijenti_prezime ON pacijenti(prezime);
CREATE INDEX idx_med_dok_pacijent ON medicinska_dokumentacija(pacijent_id);
CREATE INDEX idx_pregledi_pacijent ON pregledi(pacijent_id);
CREATE INDEX idx_pregledi_datum ON pregledi(datum_pregleda);
CREATE INDEX idx_slike_pregled ON slike(pregled_id);
CREATE INDEX idx_recepti_pacijent ON recepti(pacijent_id);
CREATE INDEX idx_recepti_datum ON recepti(datum_izdavanja);

-- Kreiranje view-a za lakše dohvaćanje podataka
CREATE OR REPLACE VIEW view_pacijent_pregled AS
SELECT 
    p.id as pacijent_id,
    p.oib,
    p.ime,
    p.prezime,
    p.datum_rodenja,
    p.spol,
    pr.id as pregled_id,
    pr.tip_pregleda,
    pr.datum_pregleda,
    pr.vrijeme_pregleda,
    pr.opis as pregled_opis,
    pr.nalaz
FROM pacijenti p
LEFT JOIN pregledi pr ON p.id = pr.pacijent_id
ORDER BY p.prezime, p.ime, pr.datum_pregleda DESC;

-- Kreiranje view-a za medicinski karton
CREATE OR REPLACE VIEW view_medicinski_karton AS
SELECT 
    p.id as pacijent_id,
    p.oib,
    p.ime,
    p.prezime,
    p.datum_rodenja,
    p.spol,
    md.id as dokumentacija_id,
    md.naziv_bolesti,
    md.datum_pocetka,
    md.datum_zavrsetka,
    md.opis as opis_bolesti,
    pr.id as pregled_id,
    pr.tip_pregleda,
    pr.datum_pregleda,
    r.id as recept_id,
    r.naziv_lijeka,
    r.doza,
    r.datum_izdavanja
FROM pacijenti p
LEFT JOIN medicinska_dokumentacija md ON p.id = md.pacijent_id
LEFT JOIN pregledi pr ON p.id = pr.pacijent_id
LEFT JOIN recepti r ON p.id = r.pacijent_id
ORDER BY p.prezime, p.ime;

-- Kreiranje funkcija za trigger-e
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Kreiranje trigger-a za automatsko ažuriranje updated_at kolona
CREATE TRIGGER update_pacijenti_updated_at BEFORE UPDATE
    ON pacijenti FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_med_dok_updated_at BEFORE UPDATE
    ON medicinska_dokumentacija FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_pregledi_updated_at BEFORE UPDATE
    ON pregledi FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_recepti_updated_at BEFORE UPDATE
    ON recepti FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Umetanje test podataka
INSERT INTO pacijenti (oib, ime, prezime, datum_rodenja, spol) VALUES
('12345678901', 'Ana', 'Anić', '1985-05-15', 'Ž'),
('23456789012', 'Marko', 'Marković', '1978-12-03', 'M'),
('34567890123', 'Petra', 'Petrić', '1992-08-22', 'Ž'),
('45678901234', 'Josip', 'Josipović', '1965-03-10', 'M'),
('56789012345', 'Iva', 'Ivić', '1990-11-18', 'Ž');

INSERT INTO medicinska_dokumentacija (pacijent_id, naziv_bolesti, datum_pocetka, datum_zavrsetka, opis) VALUES
(1, 'Hipertenzija', '2020-01-15', NULL, 'Arterijska hipertenzija, kontrolirana lijekovima'),
(2, 'Dijabetes tip 2', '2019-06-10', NULL, 'Dijabetes melitus tip 2, na metforminu'),
(3, 'Gastritis', '2023-03-05', '2023-04-15', 'Akutni gastritis, uspješno liječen'),
(1, 'Migrena', '2018-09-20', NULL, 'Kronične migrenske glavobolje');

INSERT INTO pregledi (pacijent_id, tip_pregleda, datum_pregleda, vrijeme_pregleda, opis, nalaz) VALUES
(1, 'GP', '2024-01-15', '09:30', 'Redovni pregled', 'Stabilno stanje'),
(1, 'KRV', '2024-01-16', '08:00', 'Kontrola lipida', 'Povišen kolesterol'),
(2, 'GP', '2024-02-10', '10:15', 'Kontrola dijabetesa', 'HbA1c u referentnim vrijednostima'),
(3, 'DERM', '2024-01-20', '14:30', 'Pregled kože', 'Benigne promjene'),
(4, 'EKG', '2024-02-05', '11:00', 'Kontrola srca', 'Uredan nalaz');

INSERT INTO recepti (pacijent_id, pregled_id, naziv_lijeka, doza, upute, datum_izdavanja, datum_vazenja) VALUES
(1, 1, 'Enalapril', '5mg 2x dnevno', 'Uzimati ujutro i navečer', '2024-01-15', '2024-04-15'),
(2, 3, 'Metformin', '500mg 2x dnevno', 'Uzimati uz obrok', '2024-02-10', '2024-05-10'),
(1, 2, 'Atorvastatin', '20mg navečer', 'Uzimati prije spavanja', '2024-01-16', '2024-04-16');

-- Kreiranje stored procedure za pretraživanje pacijenata
CREATE OR REPLACE FUNCTION search_patients(search_term TEXT)
RETURNS TABLE(
    id INTEGER,
    oib VARCHAR(11),
    ime VARCHAR(100),
    prezime VARCHAR(100),
    datum_rodenja DATE,
    spol CHAR(1)
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.oib, p.ime, p.prezime, p.datum_rodenja, p.spol
    FROM pacijenti p
    WHERE p.prezime ILIKE '%' || search_term || '%'
       OR p.oib = search_term
    ORDER BY p.prezime, p.ime;
END;
$$ LANGUAGE plpgsql;

-- Kreiranje stored procedure za export CSV
CREATE OR REPLACE FUNCTION export_patient_data()
RETURNS TABLE(
    oib VARCHAR(11),
    ime VARCHAR(100),
    prezime VARCHAR(100),
    datum_rodenja DATE,
    spol CHAR(1),
    broj_pregleda BIGINT,
    zadnji_pregled DATE,
    broj_recepta BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p.oib,
        p.ime,
        p.prezime,
        p.datum_rodenja,
        p.spol,
        COUNT(DISTINCT pr.id) as broj_pregleda,
        MAX(pr.datum_pregleda) as zadnji_pregled,
        COUNT(DISTINCT r.id) as broj_recepta
    FROM pacijenti p
    LEFT JOIN pregledi pr ON p.id = pr.pacijent_id
    LEFT JOIN recepti r ON p.id = r.pacijent_id
    GROUP BY p.id, p.oib, p.ime, p.prezime, p.datum_rodenja, p.spol
    ORDER BY p.prezime, p.ime;
END;
$$ LANGUAGE plpgsql;