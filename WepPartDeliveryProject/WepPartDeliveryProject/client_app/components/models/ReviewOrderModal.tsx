import React, { ChangeEvent, FC, useState } from 'react';
import { Form, Row } from 'react-bootstrap';
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import styles from '@/styles/Home.module.css'

interface ReviewOrderModelProps {
    show: boolean | undefined,
    commitAction: (review: string, rating:number) => void,
    closeModel: () => void,
}

const ReviewOrderModal: FC<ReviewOrderModelProps> = ({ show, commitAction, closeModel }) => {
    const [clientReview, setClientReview] = useState("")
    const [rating, setRating] = useState(10);

    const changeTextOfReason = (e: ChangeEvent<HTMLInputElement>) => {
        const newValue = e.target.value;
        setClientReview(newValue);
    }

    const handleCommit = async () => {
        commitAction(clientReview, rating)
    }

    const handleChangeCountClick = (countToAdd: number): void => {
        setRating((count) => {
            let sum = count + countToAdd;
            return sum > 10 || sum < 1 ? count : sum;
        });
    }

    return (
        <>
            <Modal show={show} onHide={closeModel} keyboard={false} aria-labelledby="contained-modal-title-vcenter">
                <Modal.Header closeButton>
                    <Modal.Title>Отзыв</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className="mb-3">
                            <Form.Label>Ваша оценка: </Form.Label>
                            <Row className='d-flex justify-content-center'>
                                <Button onClick={() => handleChangeCountClick(1)} className={`btn btn-secondary ${styles.cardCountBtnAndP}`}>
                                    +
                                </Button>
                                <div className={`d-flex align-items-center justify-content-center ${styles.cardCountBtnAndP}`}>
                                    <p className='m-0'>
                                        {rating}
                                    </p>
                                </div>
                                <Button onClick={() => handleChangeCountClick(-1)} className={`btn btn-secondary ${styles.cardCountBtnAndP}`}>
                                    -
                                </Button>
                            </Row>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Пожалуйста, введите отзыв: </Form.Label>
                            <Form.Control as="textarea" value={clientReview} onChange={changeTextOfReason} />
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={closeModel}>
                        Закрыть
                    </Button>
                    <Button variant="primary" onClick={handleCommit}>Оставить отзыв</Button>
                </Modal.Footer>
            </Modal>
        </>
    );
}

export default ReviewOrderModal;