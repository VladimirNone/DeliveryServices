import React, { ChangeEvent, FC, useState } from 'react';
import { Form } from 'react-bootstrap';
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';

interface CancelOrderModelProps {
    show: boolean | undefined,
    commitCancelOrder: (reasonOfCancel: string) => void,
    closeModel: () => void,
}

const CancelOrderModal: FC<CancelOrderModelProps> = ({ show, commitCancelOrder, closeModel }) => {
    const [reasonOfCancel, setReasonOfCancel] = useState("")

    const changeTextOfReason = (e:ChangeEvent<HTMLInputElement>) => {
        const newValue = e.target.value;
        setReasonOfCancel(newValue);
    }

    const handleCommit = async () => {
        commitCancelOrder(reasonOfCancel)
    }

    return (
        <>
            <Modal show={show} onHide={closeModel} keyboard={false} aria-labelledby="contained-modal-title-vcenter">
                <Modal.Header closeButton>
                    <Modal.Title>Отмена заказа</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className="mb-3">
                            <Form.Label>Пожалуйста, введите причину, из-за которой Вы отменяете заказ: </Form.Label>
                            <Form.Control as="textarea" rows={3} value={reasonOfCancel} onChange={changeTextOfReason}/>
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={closeModel}>
                        Закрыть
                    </Button>
                    <Button variant="primary" onClick={handleCommit}>Отменить заказ</Button>
                </Modal.Footer>
            </Modal>
        </>
    );
}

export default CancelOrderModal;